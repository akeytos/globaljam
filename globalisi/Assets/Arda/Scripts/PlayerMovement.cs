using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mask System")]
    public List<MaskBase> allMasks = new List<MaskBase>();
    public int selectedIndex = 0;
    public MaskBase activeMask;
    public bool isMaskEquipped = false;

    [Header("UI & Events")]
    public GameObject maskMenuCanvas; // Maske seçim menüsünün tamamı (Panel)
    public Image activeMaskIconUI;   // Menüdeki büyük maske resmi
    public UnityEvent onEquipFPS;    // R'ye basınca tetiklenecek
    public UnityEvent onUnequipTPS;  // Maskeyi çıkarınca tetiklenecek

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    // Input Actions
    private InputAction moveAction;
    private InputAction menuAction;   // E tuşu
    private InputAction selectAction; // R tuşu
    private InputAction scrollAction; // Mouse Scroll

    private bool isMenuOpen = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // --- INPUT KURULUMLARI ---
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        var wasd = moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");

        menuAction = new InputAction("Menu", InputActionType.Button, binding: "<Keyboard>/e");
        selectAction = new InputAction("Select", InputActionType.Button, binding: "<Keyboard>/r");
        scrollAction = new InputAction("Scroll", InputActionType.Value, expectedControlType: "Vector2", binding: "<Mouse>/scroll");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        menuAction.Enable();
        selectAction.Enable();
        scrollAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        menuAction.Disable();
        selectAction.Disable();
        scrollAction.Disable();
    }

    private void Update()
    {
        // Standart Hareket ve Animasyon
        Vector2 input = moveAction.ReadValue<Vector2>();
        HandleMovement(input);
        HandleAnimation(input);

        // 1. Menüyü Aç/Kapat (E Tuşu)
        if (menuAction.triggered)
        {
            ToggleMaskMenu();
        }

        // 2. Menü Açıkken Seçim ve Onay
        if (isMenuOpen)
        {
            HandleSelectionScroll();

            if (selectAction.triggered)
            {
                EquipSelectedMask();
            }
        }
    }

    private void ToggleMaskMenu()
    {
        // Eğer bir maske takılıyken E'ye basarsak maskeyi çıkarırız
        if (isMaskEquipped)
        {
            UnequipMask();
            return;
        }

        // Değilse menüyü aç/kapat
        isMenuOpen = !isMenuOpen;
        if (maskMenuCanvas != null) maskMenuCanvas.SetActive(isMenuOpen);

        // Menü açıldığında mouse'u serbest bırakmak istersen:
        // Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleSelectionScroll()
    {
        float scrollY = scrollAction.ReadValue<Vector2>().y;
        if (scrollY != 0 && allMasks.Count > 0)
        {
            if (scrollY > 0) selectedIndex = (selectedIndex + 1) % allMasks.Count;
            else selectedIndex = (selectedIndex - 1 + allMasks.Count) % allMasks.Count;

            // UI'daki resmi güncelle
            if (activeMaskIconUI != null && allMasks[selectedIndex].maskIcon != null)
                activeMaskIconUI.sprite = allMasks[selectedIndex].maskIcon;

            Debug.Log("Şu an bakıyorsun: " + allMasks[selectedIndex].maskName);
        }
    }

    private void EquipSelectedMask()
    {
        if (allMasks.Count == 0) return;

        // Maskeyi tak
        activeMask = allMasks[selectedIndex];
        activeMask.ActivateAbility(gameObject);
        isMaskEquipped = true;

        // Menüyü kapat
        isMenuOpen = false;
        if (maskMenuCanvas != null) maskMenuCanvas.SetActive(false);

        // Kamera Geçişini Tetikle (FPS'e git)
        onEquipFPS.Invoke();
        Debug.Log(activeMask.maskName + " Takıldı! FPS moduna geçiliyor...");
    }

    private void UnequipMask()
    {
        if (activeMask != null)
        {
            activeMask.DeactivateAbility(gameObject);
            activeMask = null;
        }

        isMaskEquipped = false;
        isMenuOpen = false;
        if (maskMenuCanvas != null) maskMenuCanvas.SetActive(false);

        // Kamera Geçişini Tetikle (TPS'e dön)
        onUnequipTPS.Invoke();
        Debug.Log("Maske Çıkarıldı! TPS moduna dönülüyor...");
    }

    // --- HAREKET VE ANİMASYON (DOKUNULMADI) ---
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
        if (animator == null) return;
        animator.SetBool("isWalking", input.sqrMagnitude > 0.01f);
    }
}