using UnityEngine;
using UnityEngine.UI;

public class MaskWheelController : MonoBehaviour
{
    [Header("UI Ayarlarý")]
    public GameObject wheelPanel;
    public Image[] maskIcons;

    [Header("Görsel Ayarlar")]
    public float verticalSpacing = 250f;
    public float centerScale = 1.5f;
    public float sideScale = 0.7f;
    public float animationSpeed = 10f;

    [Header("Karakter Baðlantýsý")]
    public CharacterMaskHandler characterHandler;

    private int currentIndex = 0;
    private bool isWheelOpen = false;

    void Start()
    {
        wheelPanel.SetActive(false);
        if (characterHandler == null)
            characterHandler = FindObjectOfType<CharacterMaskHandler>();
    }

    void Update()
    {
        // 1. TAB AÇ
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // ÖNCE POZÝSYONLARI SIFIRLA (Animasyon tetiklensin diye)
            ResetVisualsToCenter();

            wheelPanel.SetActive(true);
            isWheelOpen = true;
            Time.timeScale = 0.2f;
        }

        // 2. TAB KAPAT
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            wheelPanel.SetActive(false);
            isWheelOpen = false;
            Time.timeScale = 1f;

            if (characterHandler != null) characterHandler.EquipMask(currentIndex);
        }

        // 3. SCROLL VE ANÝMASYON
        if (isWheelOpen)
        {
            HandleScroll();
            UpdateMaskPositions();
        }
    }

    void HandleScroll()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll != 0)
        {
            if (scroll > 0) currentIndex--;
            else if (scroll < 0) currentIndex++;

            if (currentIndex < 0) currentIndex = maskIcons.Length - 1;
            if (currentIndex >= maskIcons.Length) currentIndex = 0;
        }
    }

    // --- YENÝ EKLENEN FONKSÝYON: HER ÞEYÝ MERKEZE TOPLA ---
    void ResetVisualsToCenter()
    {
        for (int i = 0; i < maskIcons.Length; i++)
        {
            // Hepsini tam ortaya (0,0) ýþýnla
            maskIcons[i].rectTransform.anchoredPosition = Vector2.zero;

            // Hepsini küçücük yap (Yoktan var olsunlar)
            maskIcons[i].transform.localScale = Vector3.zero;

            // Görünmez yap
            Color c = maskIcons[i].color;
            c.a = 0f;
            maskIcons[i].color = c;
        }
    }

    void UpdateMaskPositions()
    {
        for (int i = 0; i < maskIcons.Length; i++)
        {
            Vector2 targetPos = Vector2.zero;
            Vector3 targetScale = Vector3.zero;
            float targetAlpha = 0f;

            // HEDEF KONUMLARI BELÝRLE
            if (i == currentIndex) // ORTA
            {
                targetPos = Vector2.zero;
                targetScale = Vector3.one * centerScale;
                targetAlpha = 1f;
                maskIcons[i].transform.SetAsLastSibling();
            }
            else if (i == GetWrappedIndex(currentIndex - 1)) // ÜST
            {
                targetPos = new Vector2(0, verticalSpacing);
                targetScale = Vector3.one * sideScale;
                targetAlpha = 0.5f;
            }
            else if (i == GetWrappedIndex(currentIndex + 1)) // ALT
            {
                targetPos = new Vector2(0, -verticalSpacing);
                targetScale = Vector3.one * sideScale;
                targetAlpha = 0.5f;
            }
            else // DÝÐERLERÝ
            {
                targetPos = Vector2.zero;
                targetScale = Vector3.zero;
                targetAlpha = 0f;
            }

            // YUMUÞAK GEÇÝÞ (LERP)
            // ResetVisuals sayesinde hepsi 0'dan baþlayýp buradaki hedefe kayacak
            RectTransform rect = maskIcons[i].rectTransform;
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * animationSpeed);
            rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);

            Color c = maskIcons[i].color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.unscaledDeltaTime * animationSpeed);
            c.r = c.g = c.b = (i == currentIndex) ? 1f : 0.5f;
            maskIcons[i].color = c;
        }
    }

    int GetWrappedIndex(int index)
    {
        if (index < 0) return maskIcons.Length - 1;
        if (index >= maskIcons.Length) return 0;
        return index;
    }
}