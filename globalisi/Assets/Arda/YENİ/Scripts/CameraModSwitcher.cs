using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCameraSwitcher : MonoBehaviour
{
    [Header("References")]
    public Transform player;              // karakter root
    public Transform target;              // kameranın bakacağı merkez (boşsa player)
    public Transform FPSPivot;            // FPS pivot (head hizası)
    public Camera cam;

    [Header("Start Mode")]
    public bool startInTPS = true;

    [Header("TPS Orbit")]
    public float orbitDistance = 3.5f;    // SABİT
    public float orbitHeight = 1.6f;
    public float yawSpeed = 140f;
    public float pitchSpeed = 120f;
    public float minPitch = -30f;
    public float maxPitch = 70f;

    [Header("FPS Look")]
    public bool enableFPSMode = true;
    public float fpsMinPitch = -30f;
    public float fpsMaxPitch = 30f;

    [Header("Transition")]
    public float transitionDuration = 1f; // ✅ 1 saniye
    public bool lockLookDuringTransition = true;

    [Header("FOV")]
    public float tpsFov = 65f;
    public float fpsFov = 75f;

    [Header("Toggle Key")]
    public Key toggleKey = Key.V;

    // TPS angles
    private float yaw;
    private float pitch;

    // FPS angles
    private float fpsYaw;
    private float fpsPitch;

    private bool isTPS;
    private bool isTransitioning;

    // ✅ PlayerMovement buradan okuyacak
    public bool IsTransitioning => isTransitioning;

    // Transition cache
    private float transitionTimer;
    private Vector3 startPos, targetPos;
    private Quaternion startRot, targetRot;
    private float startFov, targetFov;

    // Lock angles during transition
    private float lockYaw, lockPitch, lockFpsYaw, lockFpsPitch;
    private bool lockInitialized;

    private InputAction lookAction;
    private InputAction toggleAction;

    private void Awake()
    {
        if (!player) player = transform.root;
        if (!target) target = player;
        if (!cam) cam = Camera.main;

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
        toggleAction = new InputAction("ToggleCam", InputActionType.Button,
            $"<Keyboard>/{toggleKey.ToString().ToLower()}");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        isTPS = startInTPS;

        fpsYaw = player.eulerAngles.y;
        fpsPitch = 0f;

        yaw = player.eulerAngles.y; // ✅ TPS yaw başlangıçta karakterle hizalı
        pitch = 15f;

        if (isTPS)
        {
            Vector3 center = GetTargetCenter();
            Vector3 desiredPos = CalcOrbitPosition(center, yaw, pitch, orbitDistance);
            cam.transform.position = desiredPos;
            cam.transform.rotation = Quaternion.LookRotation(center - cam.transform.position, Vector3.up);
            cam.fieldOfView = tpsFov;
        }
        else
        {
            ApplyFPSInstant();
            cam.fieldOfView = fpsFov;
        }
    }

    private void OnEnable()
    {
        lookAction.Enable();
        toggleAction.Enable();
        toggleAction.performed += OnToggle;
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnToggle;
        lookAction.Disable();
        toggleAction.Disable();
    }

    private void OnToggle(InputAction.CallbackContext ctx)
    {
        if (!enableFPSMode) return;
        StartTransition(!isTPS);
    }

    public void SwitchToFPS()
    {
        if (!enableFPSMode) return;
        if (!isTPS) return;
        StartTransition(false);
    }

    public void SwitchToTPS()
    {
        if (isTPS) return;
        StartTransition(true);
    }

    private void StartTransition(bool toTPS)
    {
        isTPS = toTPS;

        isTransitioning = true;
        transitionTimer = 0f;

        startPos = cam.transform.position;
        startRot = cam.transform.rotation;
        startFov = cam.fieldOfView;

        if (lockLookDuringTransition)
        {
            lockInitialized = false;
        }

        if (isTPS)
        {
            // ✅ FPS -> TPS
            // İSTEK: her daim karakterin baktığı yönün tam tersine (arkasına) doğru geçiş yapsın
            // Bunu sağlamak için TPS orbit yaw'ını karakter yaw'ına eşitliyoruz.
            yaw = player.eulerAngles.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Vector3 center = GetTargetCenter();

            // yaw=playerYaw olunca CalcOrbitPosition -> center + (rot * Vector3.back)*dist
            // yani karakter forward'unun tersine (arka tarafa) konum alır.
            targetPos = CalcOrbitPosition(center, yaw, pitch, orbitDistance);
            targetRot = Quaternion.LookRotation(center - targetPos, Vector3.up);
            targetFov = tpsFov;
        }
        else
        {
            // ✅ TPS -> FPS : karakterin baktığı yöne doğru dönerek FPSPivot'a aksın
            fpsYaw = player.eulerAngles.y;
            fpsPitch = 0f;

            Vector3 fpsCenter = (FPSPivot != null) ? FPSPivot.position : GetTargetCenter();
            targetPos = fpsCenter;

            Vector3 forward = player.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();

            targetRot = Quaternion.LookRotation(forward, Vector3.up);
            targetFov = fpsFov;
        }
    }

    private void Update()
    {
        if (!isTransitioning)
        {
            HandleLook();
        }
        else if (lockLookDuringTransition)
        {
            LockAnglesWhileTransition();
        }

        HandleTransitionOrApply();
    }

    private void HandleLook()
    {
        Vector2 d = lookAction.ReadValue<Vector2>();

        if (isTPS)
        {
            yaw += d.x * (yawSpeed * 0.01f);
            pitch -= d.y * (pitchSpeed * 0.01f);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            fpsYaw += d.x * (yawSpeed * 0.01f);
            fpsPitch -= d.y * (pitchSpeed * 0.01f);
            fpsPitch = Mathf.Clamp(fpsPitch, fpsMinPitch, fpsMaxPitch);
        }
    }

    private void HandleTransitionOrApply()
    {
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);

            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            cam.fieldOfView = Mathf.Lerp(startFov, targetFov, t);

            if (t >= 1f)
            {
                isTransitioning = false;

                // Transition bittiğinde FPS'e geçtiysek pivot'a net oturt
                if (!isTPS)
                {
                    ApplyFPSInstant();
                    cam.fieldOfView = fpsFov;
                }
            }

            return;
        }

        if (isTPS)
        {
            Vector3 center = GetTargetCenter();
            Vector3 desiredPos = CalcOrbitPosition(center, yaw, pitch, orbitDistance);
            cam.transform.position = desiredPos;

            cam.transform.rotation = Quaternion.LookRotation(center - cam.transform.position, Vector3.up);
            cam.fieldOfView = tpsFov;
        }
        else
        {
            ApplyFPS();
            cam.fieldOfView = fpsFov;
        }
    }

    private Vector3 GetTargetCenter()
    {
        Vector3 basePos = target != null ? target.position : player.position;
        return basePos + Vector3.up * orbitHeight;
    }

    private Vector3 CalcOrbitPosition(Vector3 center, float y, float p, float dist)
    {
        Quaternion rot = Quaternion.Euler(p, y, 0f);
        Vector3 offsetDir = rot * Vector3.back;
        return center + offsetDir * dist;
    }

    private void ApplyFPSInstant()
    {
        Vector3 fpsPos = (FPSPivot != null) ? FPSPivot.position : GetTargetCenter();
        cam.transform.position = fpsPos;

        player.rotation = Quaternion.Euler(0f, fpsYaw, 0f);
        cam.transform.rotation = Quaternion.Euler(fpsPitch, fpsYaw, 0f);
    }

    private void ApplyFPS()
    {
        Vector3 fpsPos = (FPSPivot != null) ? FPSPivot.position : GetTargetCenter();
        cam.transform.position = fpsPos;

        player.rotation = Quaternion.Euler(0f, fpsYaw, 0f);
        cam.transform.rotation = Quaternion.Euler(fpsPitch, fpsYaw, 0f);
    }

    private void LockAnglesWhileTransition()
    {
        if (!lockInitialized)
        {
            lockYaw = yaw;
            lockPitch = pitch;
            lockFpsYaw = fpsYaw;
            lockFpsPitch = fpsPitch;
            lockInitialized = true;
        }

        yaw = lockYaw;
        pitch = lockPitch;
        fpsYaw = lockFpsYaw;
        fpsPitch = lockFpsPitch;
    }
}
