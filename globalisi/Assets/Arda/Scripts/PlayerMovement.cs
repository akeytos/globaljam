using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections; // Coroutine için gerekli
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -19.62f;
    public float jumpHeight = 2f;

    [Header("Deer (Geyik) Dash Settings")]
    public bool canDash = false; // Geyik maskesi bunu 'true' yapar
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
    public bool canClimb = false;
    public bool isClimbingNow = false;

    [Header("FPS/TPS Events")]
    public UnityEvent onEquipFPS;
    public UnityEvent onUnequipTPS;

    [Header("References")]
    public MaskWheelController wheelController;
    public Transform cameraTransform;
    public InputAction moveAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isMenuOpen = false;
    private LayerMask climbableLayer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        climbableLayer = LayerMask.GetMask("Climbable");

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

        HandleInput();
        Vector2 input = moveAction.ReadValue<Vector2>();

        // Dash Cooldown takibi
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.unscaledDeltaTime;

        if (canClimb) CheckForWallClimb(input.y);
        else isClimbingNow = false;

        // Dashing anında normal hareketi durduruyoruz
        if (!isClimbingNow && !isDashing) HandleNormalMovement(input);
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

        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Zıplama (Space)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity) * timeComp;
        }

        velocity.y += gravity * timeComp * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleInput()
    {
        // --- DASH TUŞU: X ---
        if (canDash && Input.GetKeyDown(KeyCode.X) && !isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(DashAction());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMaskEquipped) UnequipMask();
            else if (wheelController != null) { isMenuOpen = !isMenuOpen; wheelController.SetMenuState(isMenuOpen); }
        }
        if (isMenuOpen && Input.GetKeyDown(KeyCode.R)) EquipCurrentSelected();
    }

    private IEnumerator DashAction()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        Debug.Log("<color=brown>Geyik Atılması!</color>");

        // Dash yönünü belirle
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 dashDir = (cameraTransform.right * input.x) + (cameraTransform.forward * input.y);
        if (dashDir.sqrMagnitude < 0.01f) dashDir = transform.forward;
        dashDir.y = 0;
        dashDir.Normalize();

        // Baykuş maskesi takılıysa dash hızını da zaman ölçeğine göre ayarla
        float dashComp = (Time.timeScale < 1f) ? (1f / Time.timeScale) : 1f;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            controller.Move(dashDir * dashSpeed * dashComp * Time.deltaTime);
            elapsed += Time.unscaledDeltaTime; // Zaman bükülse de dash süresi gerçek zamanlı kalır
            yield return null;
        }

        isDashing = false;
    }

    // [CheckForWallClimb, Equip/Unequip fonksiyonları buraya gelecek - aynı kalsın]
    private void CheckForWallClimb(float vInput)
    {
        Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * 0.4f;
        RaycastHit hit;
        if (Physics.Raycast(origin, transform.forward, out hit, 0.8f, climbableLayer))
        {
            isClimbingNow = true;
            velocity = Vector3.zero;
            controller.stepOffset = 0f;
            float climbComp = (Time.timeScale < 1f) ? (1f / Time.timeScale) : 1f;
            Vector3 climbMove = new Vector3(0, vInput * 4f * climbComp, 0) + (transform.forward * 0.2f);
            controller.Move(climbMove * Time.deltaTime);
        }
        else if (isClimbingNow) { isClimbingNow = false; controller.stepOffset = 0.3f; }
    }

    private void EquipCurrentSelected()
    {
        if (wheelController == null) return;
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
        }
    }

    public void UnequipMask()
    {
        if (activeMask != null) activeMask.DeactivateAbility(gameObject);
        activeMask = null;
        isMaskEquipped = false;
        canClimb = false;
        isClimbingNow = false;
        isMenuOpen = false;
        if (wheelController != null) wheelController.SetMenuState(false);
        onUnequipTPS.Invoke();
    }
}