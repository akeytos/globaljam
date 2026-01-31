using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mask System")]
    public List<MaskBase> allMasks = new List<MaskBase>();
    public MaskBase activeMask;
    public bool isMaskEquipped = false;
    private bool isMenuOpen = false;

    [Header("Camera Events")]
    public UnityEvent onEquipFPS;
    public UnityEvent onUnequipTPS;

    [Header("References")]
    public MaskWheelController wheelController; // Hata burada, bu boş kalıyor!
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private InputAction moveAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        var wasd = moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    private void Start()
    {
        onUnequipTPS.Invoke(); // TPS Başla

        // --- HATA DÜZELTİCİ KOD ---
        // Eğer editörden sürüklemeyi unuttuysan, oyun başlayınca otomatik bulsun:
        if (wheelController == null)
        {
            wheelController = FindFirstObjectByType<MaskWheelController>();

            if (wheelController == null)
            {
                Debug.LogError("KANKA DİKKAT: Sahneye 'MaskSystem' (MaskWheelController) eklememişsin!");
            }
        }
    }

    private void Update()
    {
        HandleInput();

        Vector2 input = moveAction.ReadValue<Vector2>();
        HandleMovement(input);
        HandleAnimation(input);
    }

    private void HandleInput()
    {
        // 1. E TUŞU: MENÜ AÇ/KAPAT VEYA MASKE ÇIKAR
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMaskEquipped)
            {
                UnequipMask();
            }
            else
            {
                // wheelController null ise burada patlıyordu, artık patlamaz.
                if (wheelController != null)
                {
                    isMenuOpen = !isMenuOpen;
                    wheelController.SetMenuState(isMenuOpen);
                }
            }
        }

        // 2. R TUŞU: MASKE KUŞAN (SADECE MENÜ AÇIKKEN)
        if (isMenuOpen && Input.GetKeyDown(KeyCode.R))
        {
            EquipCurrentSelected();
        }
    }

    private void EquipCurrentSelected()
    {
        if (wheelController == null) return;

        int index = wheelController.GetCurrentIndex();

        if (activeMask != null) activeMask.DeactivateAbility(gameObject);

        // Listede eleman var mı kontrolü (Hata önleyici)
        if (index >= 0 && index < allMasks.Count)
        {
            activeMask = allMasks[index];
            activeMask.ActivateAbility(gameObject);
            isMaskEquipped = true;

            // Menüyü kapat ve geçişi başlat
            isMenuOpen = false;
            wheelController.SetMenuState(false);
            onEquipFPS.Invoke();

            Debug.Log(activeMask.maskName + " Formuna Girildi (R)");
        }
    }

    public void UnequipMask()
    {
        if (activeMask != null) activeMask.DeactivateAbility(gameObject);
        activeMask = null;
        isMaskEquipped = false;
        isMenuOpen = false;

        if (wheelController != null)
            wheelController.SetMenuState(false);

        onUnequipTPS.Invoke();
    }

    private void HandleMovement(Vector2 input)
    {
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

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleAnimation(Vector2 input)
    {
        if (animator != null) animator.SetBool("isWalking", input.sqrMagnitude > 0.01f);
    }
}