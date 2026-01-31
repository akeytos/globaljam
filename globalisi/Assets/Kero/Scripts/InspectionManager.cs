using UnityEngine;

public class InspectionManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform inspectionPoint;
    public float rotationSpeed = 2f; // Inspector'da bunu kontrol et!

    [Header("Referanslar")]
    public PlayerMovement playerMovementScript;
    public CameraModeSwitcher cameraLookScript;
    public GameObject crosshairUI;
    public GameObject interactionTextUI;
    public GameObject blurPanelUI;

    [HideInInspector]
    public bool isInspecting = false;

    private GameObject currentModel;
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        if (isInspecting && currentModel != null)
        {
            // --- YENÝ EKSEN SÝSTEMÝ (DÜNYA SALLANMASIN DÝYE) ---
            if (Input.GetMouseButton(0))
            {
                // Fare hareketini al
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

                // 1. ADIM: Maskeyi olduðu yerde, KAMERANIN saðýna/soluna göre çevir
                // (Yani sen kafaný saða çevirir gibi)
                currentModel.transform.Rotate(camTransform.up, -mouseX, Space.World);

                // 2. ADIM: Maskeyi KAMERANIN saðý ekseninde yukarý/aþaðý çevir
                // (Yani kafaný öne eðer gibi)
                currentModel.transform.Rotate(camTransform.right, mouseY, Space.World);
            }

            // ÇIKIÞ
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseInspection();
            }
        }
    }

    public void OpenInspection(GameObject maskPrefab)
    {
        if (isInspecting) return;
        isInspecting = true;

        // Kontrolleri kapat
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (cameraLookScript != null) cameraLookScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshairUI != null) crosshairUI.SetActive(false);
        if (interactionTextUI != null) interactionTextUI.SetActive(false);
        if (blurPanelUI != null) blurPanelUI.SetActive(true);

        // --- MASKE POZÝSYON AYARI ---
        // Maskeyi oluþtur
        currentModel = Instantiate(maskPrefab, inspectionPoint.position, inspectionPoint.rotation);

        // ÖNEMLÝ: Maske tam kameraya baksýn
        currentModel.transform.LookAt(camTransform);

        // Ebeveyn yap
        currentModel.transform.SetParent(inspectionPoint);

        // Temizlik
        Destroy(currentModel.GetComponent<BoxCollider>());
        Destroy(currentModel.GetComponent<InteractableObject>());
        if (currentModel.GetComponent<Rigidbody>()) Destroy(currentModel.GetComponent<Rigidbody>());
    }

    public void CloseInspection()
    {
        isInspecting = false;

        // Kontrolleri aç
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (cameraLookScript != null) cameraLookScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairUI != null) crosshairUI.SetActive(true);
        if (blurPanelUI != null) blurPanelUI.SetActive(false);

        if (currentModel != null) Destroy(currentModel);
    }
}