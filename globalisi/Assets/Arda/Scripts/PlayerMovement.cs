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

    [Header("References")]
    public MaskWheelController wheelController;
    public Transform cameraTransform;
    public InputAction moveAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isMenuOpen = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
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
        Vector2 input = moveAction.ReadValue<Vector2>();

        HandleInput();
        HandleWebAction(input);

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.unscaledDeltaTime;

        if (isWebbed)
        {
            ExecuteWebMove();
        }
        else if (!isDashing)
        {
            HandleNormalMovement(input);
        }
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity) * timeComp;

        velocity.y += gravity * timeComp * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleWebAction(Vector2 moveInput)
    {
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

                if (webLine != null)
                {
                    webLine.enabled = true;
                    webLine.SetPosition(0, transform.position);
                    webLine.SetPosition(1, webTargetPoint);
                }
            }
        }

        // WASD'ye basarsan veya maskeyi çıkarırsan bağı kopar
        if (isWebbed && moveInput.sqrMagnitude > 0.01f)
        {
            StopWebbing();
        }
    }

    private void ExecuteWebMove()
    {
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
            // Hedefe vardık, orada asılı kalıyoruz (isWebbed hala true)
        }
    }

    public void StopWebbing()
    {
        isWebbed = false;
        controller.enabled = true;
        if (webLine != null) webLine.enabled = false;
        velocity = Vector3.zero;
    }

    private void HandleInput()
    {
        if (canDash && Input.GetKeyDown(KeyCode.X) && !isDashing && dashCooldownTimer <= 0)
            StartCoroutine(DashAction());

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
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 dashDir = (cameraTransform.right * input.x) + (cameraTransform.forward * input.y);
        if (dashDir.sqrMagnitude < 0.01f) dashDir = transform.forward;
        dashDir.y = 0; dashDir.Normalize();
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            controller.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        isDashing = false;
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

            var visionUI = FindObjectOfType<MaskVisionUI>();
            if (visionUI != null) visionUI.ShowVision(activeMask.maskOverlay, activeMask.maskTintColor);
        }
    }

    public void UnequipMask()
    {
        StopWebbing();
        if (activeMask != null) activeMask.DeactivateAbility(gameObject);
        var visionUI = FindObjectOfType<MaskVisionUI>();
        if (visionUI != null) visionUI.HideVision();

        activeMask = null;
        isMaskEquipped = false;
        canWeb = false;
        isMenuOpen = false;
        if (wheelController != null) wheelController.SetMenuState(false);
        onUnequipTPS.Invoke();
    }
}