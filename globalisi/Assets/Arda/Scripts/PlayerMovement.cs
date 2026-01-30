using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform;   // Main Camera veya CameraPivot'un parent'i
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    // Input System (koda gömülü)
    private InputAction moveAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Move action'ı kodda oluştur
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

        var wasd = moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w");
        wasd.With("Down", "<Keyboard>/s");
        wasd.With("Left", "<Keyboard>/a");
        wasd.With("Right", "<Keyboard>/d");
    }

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        HandleMovement(input);
        HandleAnimation(input);
    }

    private void HandleMovement(Vector2 input)
    {
        // Kamera yoksa fallback: world ekseniyle yürü
        Vector3 moveDir;

        if (cameraTransform != null)
        {
            // Kameranın forward/right vektörlerini yere projeksiyonla (y = 0)
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            // W/S = camForward, A/D = camRight
            moveDir = (camRight * input.x) + (camForward * input.y);
        }
        else
        {
            moveDir = new Vector3(input.x, 0f, input.y);
        }

        // Çapraz hız fix
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleAnimation(Vector2 input)
    {
        if (animator == null) return;

        bool isWalking = input.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);
    }
}
