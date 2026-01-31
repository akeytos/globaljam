using UnityEngine;
using UnityEngine.InputSystem;

public class CameraModeSwitcher : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // yaw burada (sağa-sola)
    public Transform cameraPivot;     // TPS pitch burada
    public Camera cam;               // FPS pitch burada

    [Header("Start Mode")]
    public bool startInTPS = false;

    [Header("Offsets (Local)")]
    public Vector3 fpsLocalOffset = Vector3.zero;
    public Vector3 tpsLocalOffset = new Vector3(0f, 0.6f, -3.5f);

    [Header("Look")]
    public float mouseSensitivity = 0.08f;

    [Header("Pitch Limits (FPS)")]
    public float fpsMinPitch = -30f;
    public float fpsMaxPitch = 30f;

    [Header("Pitch Limits (TPS)")]
    public float tpsMinPitch = -70f;
    public float tpsMaxPitch = 80f;

    [Header("Transition")]
    public float transitionDuration = 0.5f; // Test için biraz hızlandırdım (opsiyonel)

    [Header("FOV")]
    public float fpsFov = 75f;
    public float tpsFov = 65f;

    private bool isTPS;
    private bool isTransitioning;

    private float yaw;
    private float pitch;

    private float transitionTimer;
    private Vector3 transitionStartPos;
    private Vector3 transitionTargetPos;
    private float startFov;
    private float targetFov;

    private InputAction lookAction;
    private InputAction toggleAction;

    // --- MASKE SİSTEMİ İÇİN EKLEDİĞİMİZ PUBLİC FONKSİYONLAR ---
    public void SetFPSView()
    {
        if (isTPS) StartTransition(false); // Eğer TPS ise FPS'e geç
    }

    public void SetTPSView()
    {
        if (!isTPS) StartTransition(true); // Eğer FPS ise TPS'e geç
    }
    // -------------------------------------------------------

    private void Awake()
    {
        if (!player) player = transform.root;
        if (!cameraPivot) cameraPivot = transform;
        if (!cam) cam = Camera.main;

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        toggleAction = new InputAction("ToggleCam", InputActionType.Button, "<Keyboard>/v");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        yaw = player.eulerAngles.y;
        float camX = cam.transform.localEulerAngles.x;
        if (camX > 180f) camX -= 360f;

        isTPS = startInTPS;
        isTransitioning = false;

        pitch = isTPS
            ? Mathf.Clamp(camX, tpsMinPitch, tpsMaxPitch)
            : Mathf.Clamp(camX, fpsMinPitch, fpsMaxPitch);

        ApplyLookInstant(isTPS);

        cam.transform.localPosition = isTPS ? tpsLocalOffset : fpsLocalOffset;
        cam.fieldOfView = isTPS ? tpsFov : fpsFov;
    }

    private void OnEnable()
    {
        lookAction.Enable();
        toggleAction.Enable();
        toggleAction.performed += OnToggleInput;
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnToggleInput;
        lookAction.Disable();
        toggleAction.Disable();
    }

    private void OnToggleInput(InputAction.CallbackContext ctx)
    {
        StartTransition(!isTPS);
    }

    private void StartTransition(bool targetIsTPS)
    {
        isTPS = targetIsTPS;
        isTransitioning = true;
        transitionTimer = 0f;

        transitionStartPos = cam.transform.localPosition;
        transitionTargetPos = isTPS ? tpsLocalOffset : fpsLocalOffset;

        startFov = cam.fieldOfView;
        targetFov = isTPS ? tpsFov : fpsFov;

        pitch = Mathf.Clamp(pitch, tpsMinPitch, tpsMaxPitch);
    }

    private void Update()
    {
        HandleLook();
        HandleTransition();
    }

    private void HandleLook()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;

        bool useTPSRotation = isTransitioning ? true : isTPS;

        if (useTPSRotation) pitch = Mathf.Clamp(pitch, tpsMinPitch, tpsMaxPitch);
        else pitch = Mathf.Clamp(pitch, fpsMinPitch, fpsMaxPitch);

        player.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (useTPSRotation)
        {
            if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            cam.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if (cameraPivot != null) cameraPivot.localRotation = Quaternion.identity;
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void HandleTransition()
    {
        if (!isTransitioning) return;

        transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(transitionTimer / transitionDuration);

        cam.transform.localPosition = Vector3.Lerp(transitionStartPos, transitionTargetPos, t);
        cam.fieldOfView = Mathf.Lerp(startFov, targetFov, t);

        if (t >= 1f)
        {
            isTransitioning = false;
            ApplyLookInstant(isTPS);
            if (!isTPS) pitch = Mathf.Clamp(pitch, fpsMinPitch, fpsMaxPitch);
        }
    }

    private void ApplyLookInstant(bool useTPSRotation)
    {
        player.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (useTPSRotation)
        {
            if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            cam.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if (cameraPivot != null) cameraPivot.localRotation = Quaternion.identity;
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}