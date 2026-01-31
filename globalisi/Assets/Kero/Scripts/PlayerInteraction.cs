using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Ayarlar")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("UI Baðlantýsý")]
    public GameObject interactionTextObj;
    public TextMeshProUGUI interactionText;

    [Header("Ýnceleme Sistemi (YENÝ)")]
    public InspectionManager inspectionManager; // <-- Bunu editörde baðlayacaðýz

    private InteractableObject currentInteractable;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Eðer þu an inceleme modundaysak arkada ýþýn atýp durma, bekle
        if (inspectionManager != null && inspectionManager.isInspecting) return;

        CheckForInteractable();

        // --- Q TUÞU ÝLE ÝNCELEME BAÞLAT (YENÝ KISIM) ---
        if (Input.GetKeyDown(KeyCode.Q) && currentInteractable != null)
        {
            if (inspectionManager != null)
            {
                // Yöneticisine "Al bu maskeyi ekrana getir" diyoruz
                inspectionManager.OpenInspection(currentInteractable.gameObject);

                // UI'ý temizle ki ekranda yazý kalmasýn
                ClearInteraction();
            }
        }
    }

    void CheckForInteractable()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    ClearInteraction();
                    currentInteractable = interactable;
                    currentInteractable.OnLookEnter(); // Parlat

                    if (interactionTextObj != null) interactionTextObj.SetActive(true);
                    if (interactionText != null) interactionText.text = "[Q] Ýncele: " + interactable.itemName;
                }
                return;
            }
        }

        ClearInteraction();
    }

    void ClearInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLookExit(); // Söndür
            currentInteractable = null;
        }

        if (interactionTextObj != null) interactionTextObj.SetActive(false);
    }
}