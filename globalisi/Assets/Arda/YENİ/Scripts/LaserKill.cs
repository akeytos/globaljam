using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LaserKill : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float restartDelay = 0.1f;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        StartCoroutine(RestartScene());
    }

    private IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
