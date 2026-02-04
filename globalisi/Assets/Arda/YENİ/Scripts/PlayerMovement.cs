using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -19.62f;
    public float jumpHeight = 2f;

    [Header("Spider Web (Örümcek Ağı) Settings")]
    public float webZipSpeed = 25f;
    public float webMaxDistance = 40f;
    public LayerMask webableLayer; // Ağın yapışacağı katmanlar
    public LineRenderer webLine;   // Görsel ağ için LineRenderer
    private bool isWebbed = false;
    private Vector3 webTargetPoint;

    [Header("Deer (Geyik) Settings")]
    public bool canDash = false;
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    [Header("Mask System")]
    public List<MaskBase> allMasks = new List<MaskBase>();
    public MaskBase activeMask;
    public bool isMaskEquipped = false;
    // canClimb artık "Ağ Atma" yeteneği olarak kullanılıyor
    public bool canWeb = false;

    [Header("FPS/TPS Events")]
    public UnityEvent onEquipFPS;
    public UnityEvent onUnequipTPS;

    [Header("Animation")]
    public Animator animator;
    [Tooltip("Animator içindeki yürüyüş bool parametresinin adı. Varsayılan: Walk")]
    public string walkBoolName = "Walk";

    // ✅ Jump anim parametreleri
    [Tooltip("Animator içindeki zıplama trigger parametresinin adı. Varsayılan: Jump")]
    public string jumpTriggerName = "Jump";
    [Tooltip("Animator içindeki yerde mi bool parametresinin adı. Varsayılan: IsGrounded")]
    public string groundedBoolName = "IsGrounded";

    // ✅ Punch anim parametresi
    [Tooltip("Animator içindeki punch trigger parametresinin adı. Varsayılan: Punch")]
    public string punchTriggerName = "Punch";

    [Header("Punch Lock")]
    [Tooltip("Punch animasyonu boyunca karakterin kilitli kalacağı süre (saniye). Clip süren neyse ona yakın yap. Örn: 0.55")]
    public float punchLockDuration = 0.55f;
    private bool isPunching = false;
    private Coroutine punchRoutine;

    [Header("References")]
    public MaskWheelController wheelController;
    public Transform cameraTransform;
    public InputAction moveAction;

    // ✅ Maske takınca otomatik FPS'e geçmek için kamera script referansı
    public OrbitCameraSwitcher cameraSwitcher;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isMenuOpen = false;

    // Animator bulunamazsa spam olmasın diye 1 kere uyarı basıyoruz
    private bool warnedAnimatorMissing = false;

    // ✅ ROTATION
    [Header("Rotation")]
    [Tooltip("Karakterin hareket yönüne dönme hızı (yüksek = daha hızlı döner).")]
    public float rotationSpeed = 12f;

    // ✅ Transition sırasında hareket kilidi
    private bool IsInMaskTransition()
    {
        return cameraSwitcher != null && cameraSwitcher.IsTransitioning;
    }

    // ✅ Genel “kilit” (transition veya punch)
    private bool IsHardLocked()
    {
        return IsInMaskTransition() || isPunching;
    }

    private void SetWalk(bool value)
    {
        if (animator == null)
        {
            if (!warnedAnimatorMissing)
            {
                warnedAnimatorMissing = true;
                Debug.LogWarning($"[PlayerMovement] Animator bulunamadı. " +
                                 $"AnılPlayer parent'ta script var ama Animator child'da olabilir. " +
                                 $"Inspector'dan bağla veya otomatik bulması için child'da Animator olduğundan emin ol. (GameObject: {name})");
            }
            return;
        }

        animator.SetBool(walkBoolName, value);
    }

    // ✅ Grounded bool güncelle
    private void SetGrounded(bool value)
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(groundedBoolName))
            animator.SetBool(groundedBoolName, value);
    }

    // ✅ Jump trigger tetikle
    private void TriggerJump()
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(jumpTriggerName))
            animator.SetTrigger(jumpTriggerName);
    }

    // ✅ Punch trigger tetikle
    private void TriggerPunch()
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(punchTriggerName))
            animator.SetTrigger(punchTriggerName);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // ✅ Script parent'ta, Animator child'daysa otomatik bul
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        // ✅ Inspector'dan verilmediyse otomatik bul
        if (cameraSwitcher == null)
        {
            cameraSwitcher = FindObjectOfType<OrbitCameraSwitcher>();
        }

        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        var wasd = moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // ✅ Animator grounded bool her frame güncellensin
        SetGrounded(isGrounded);

        // ✅ Transition veya Punch sırasında: input yok, dash/web/jump yok, sadece gravity çalışsın
        if (IsHardLocked())
        {
            SetWalk(false);
            ApplyGravityOnly();
            return;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();

        HandleInput();
        HandleWebAction(input);

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.unscaledDeltaTime;

        // ✅ Menü / web / dash sırasında yürüyüş animasyonu kapalı kalsın
        if (isMenuOpen || isWebbed || isDashing)
            SetWalk(false);

        if (isWebbed)
        {
            ExecuteWebMove();
        }
        else if (!isDashing)
        {
            HandleNormalMovement(input);
        }
    }

    // ✅ Transition/Punch esnasında sadece düşüş/yer çekimi çalışsın
    private void ApplyGravityOnly()
    {
        float timeComp = (Time.timeScale < 1f) ? (1f / Time.timeScale) : 1f;

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        velocity.y += gravity * timeComp * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleNormalMovement(Vector2 input)
    {
        float timeComp = (Time.timeScale < 1f) ? (1f / Time.timeScale) : 1f;
        float currentSpeed = moveSpeed * timeComp;

        Vector3 moveDir;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f; camRight.y = 0f;
            camForward.Normalize(); camRight.Normalize();
            moveDir = (camRight * input.x) + (camForward * input.y);
        }
        else moveDir = new Vector3(input.x, 0f, input.y);

        // ✅ ROTATION: hareket yönüne dön
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // ✅ Walk true/false
        bool isWalking = moveDir.sqrMagnitude > 0.01f;
        SetWalk(isWalking);

        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // ✅ Jump + Jump anim trigger
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity) * timeComp;

            SetWalk(false);
            TriggerJump();
            SetGrounded(false);
        }

        velocity.y += gravity * timeComp * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleWebAction(Vector2 moveInput)
    {
        // ✅ Punch/Transition sırasında web olmasın
        if (IsHardLocked())
            return;

        // Sadece Örümcek maskesi varken Sol Tık
        if (canWeb && Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, webMaxDistance, webableLayer))
            {
                webTargetPoint = hit.point;
                isWebbed = true;
                controller.enabled = false; // Yapışma anında fizikleri kapat

                SetWalk(false);

                if (webLine != null)
                {
                    webLine.enabled = true;
                    webLine.SetPosition(0, transform.position);
                    webLine.SetPosition(1, webTargetPoint);
                }
            }
        }

        // WASD'ye basarsan bağı kopar
        if (isWebbed && moveInput.sqrMagnitude > 0.01f)
        {
            StopWebbing();
        }
    }

    private void ExecuteWebMove()
    {
        SetWalk(false);

        Vector3 direction = (webTargetPoint - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, webTargetPoint);

        // Hedefe 0.8 metre kalana kadar çekil
        if (distance > 0.8f)
        {
            transform.position += direction * webZipSpeed * Time.deltaTime;
            if (webLine != null) webLine.SetPosition(0, transform.position);
        }
        else
        {
            // Hedefe vardık, orada asılı kalıyoruz
        }
    }

    public void StopWebbing()
    {
        isWebbed = false;
        controller.enabled = true;
        if (webLine != null) webLine.enabled = false;
        velocity = Vector3.zero;

        SetWalk(false);
    }

    private void HandleInput()
    {
        // ✅ Transition veya Punch sırasında input kesinlikle işlenmesin
        if (IsHardLocked())
            return;

        // ✅ PUNCH: Sol tık (Ama canWeb varsa web kullanıyor, punch tetiklemiyoruz)
        if (!canWeb && Input.GetMouseButtonDown(0))
        {
            // Menü/web/dash sırasında punch olmasın
            if (isMenuOpen || isWebbed || isDashing)
                return;

            StartPunchLock(); // ✅ hareketi kilitle + anim tetikle
            return;
        }

        if (canDash && Input.GetKeyDown(KeyCode.X) && !isDashing && dashCooldownTimer <= 0)
            StartCoroutine(DashAction());

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMaskEquipped) UnequipMask();
            else if (wheelController != null)
            {
                isMenuOpen = !isMenuOpen;
                wheelController.SetMenuState(isMenuOpen);

                if (isMenuOpen) SetWalk(false);
            }
        }

        if (isMenuOpen && Input.GetKeyDown(KeyCode.R)) EquipCurrentSelected();
    }

    private void StartPunchLock()
    {
        // Zaten punch atıyorsak tekrar başlatma
        if (isPunching) return;

        SetWalk(false);

        // Punch animini tetikle
        TriggerPunch();

        // Punch sırasında hareketi kilitle
        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(PunchLockRoutine());
    }

    private IEnumerator PunchLockRoutine()
    {
        isPunching = true;

        // Yumruk sırasında web/dash vs olmasın diye güvenlik
        SetWalk(false);

        float elapsed = 0f;
        while (elapsed < punchLockDuration)
        {
            // Transition başladıysa zaten genel lock devreye giriyor
            elapsed += Time.deltaTime;
            yield return null;
        }

        isPunching = false;
        punchRoutine = null;
    }

    private IEnumerator DashAction()
    {
        isDashing = true;
        SetWalk(false);

        dashCooldownTimer = dashCooldown;
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 dashDir = (cameraTransform.right * input.x) + (cameraTransform.forward * input.y);
        if (dashDir.sqrMagnitude < 0.01f) dashDir = transform.forward;
        dashDir.y = 0; dashDir.Normalize();

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            // ✅ Transition veya Punch başladıysa dash de bitsin
            if (IsHardLocked()) break;

            controller.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        isDashing = false;
        SetWalk(false);
    }

    private void EquipCurrentSelected()
    {
        int index = wheelController.GetCurrentIndex();
        if (activeMask != null) activeMask.DeactivateAbility(gameObject);

        if (index < allMasks.Count)
        {
            activeMask = allMasks[index];
            activeMask.ActivateAbility(gameObject);
            isMaskEquipped = true;
            isMenuOpen = false;
            wheelController.SetMenuState(false);
            onEquipFPS.Invoke();

            // ✅ Maske takınca otomatik FPS'e geç
            if (cameraSwitcher != null) cameraSwitcher.SwitchToFPS();

            var visionUI = FindObjectOfType<MaskVisionUI>();
            if (visionUI != null) visionUI.ShowVision(activeMask.maskOverlay, activeMask.maskTintColor);
        }
    }

    public void UnequipMask()
    {
        StopWebbing();
        SetWalk(false);

        // ✅ Punch kilidini de kes (mask çıkarırken garip kalmasın)
        isPunching = false;
        if (punchRoutine != null) { StopCoroutine(punchRoutine); punchRoutine = null; }

        if (activeMask != null) activeMask.DeactivateAbility(gameObject);
        var visionUI = FindObjectOfType<MaskVisionUI>();
        if (visionUI != null) visionUI.HideVision();

        activeMask = null;
        isMaskEquipped = false;
        canWeb = false;
        isMenuOpen = false;
        if (wheelController != null) wheelController.SetMenuState(false);
        onUnequipTPS.Invoke();

        // ✅ Maske çıkarınca otomatik TPS'e dön
        if (cameraSwitcher != null) cameraSwitcher.SwitchToTPS();
    }
}
