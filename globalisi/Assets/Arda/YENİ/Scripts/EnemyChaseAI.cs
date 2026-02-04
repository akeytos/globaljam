using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaseAI : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Takip edilecek hedef. Boş bırakılırsa tag'i Player olan objeyi bulur.")]
    public Transform target;

    [Header("Chase Settings")]
    public float updateRate = 0.1f;          // hedefi kaç saniyede bir güncellesin
    public float chaseRange = 999f;          // istersen menzil koy
    public float stopDistance = 1.2f;        // hedefe yaklaşınca duracağı mesafe

    [Header("Attack / Kill")]
    public bool killOnTouch = true;          // temas edince sahneyi resetlesin mi?
    public float killCooldown = 0.5f;        // üst üste tetiklenmesin
    public string playerTag = "Player";      // player tag
    public string sceneToReload = "";        // boşsa aktif sahneyi reload eder

    [Header("Debug")]
    public bool drawGizmos = true;

    private NavMeshAgent agent;
    private float timer;
    private float lastKillTime = -999f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) target = p.transform;
        }
    }

    private void Update()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // Menzil dışındaysa dur
        if (dist > chaseRange)
        {
            if (!agent.isStopped) agent.isStopped = true;
            return;
        }

        if (agent.isStopped) agent.isStopped = false;

        // Hedefi belirli aralıkla güncelle
        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            timer = 0f;
            agent.SetDestination(target.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!killOnTouch) return;
        if (!other.CompareTag(playerTag)) return;

        if (Time.time - lastKillTime < killCooldown) return;
        lastKillTime = Time.time;

        ReloadScene();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Trigger yerine collider kullanıyorsan bu da çalışır
        if (!killOnTouch) return;
        if (!collision.collider.CompareTag(playerTag)) return;

        if (Time.time - lastKillTime < killCooldown) return;
        lastKillTime = Time.time;

        ReloadScene();
    }

    private void ReloadScene()
    {
        if (!string.IsNullOrEmpty(sceneToReload))
        {
            SceneManager.LoadScene(sceneToReload);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
