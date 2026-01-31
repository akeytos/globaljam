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

    [Header("Ses Ayarlarý")]
    public AudioSource uiAudioSource; // MaskSystem üzerindeki Audio Source
    public AudioClip scrollSound;     // Týk sesi

    [Header("Karakter Baðlantýsý")]
    public CharacterMaskHandler characterHandler;

    private int currentIndex = 0;
    private bool isWheelOpen = false;

    void Start()
    {
        // Baþlangýçta paneli kapat
        if (wheelPanel != null)
            wheelPanel.SetActive(false);

        // Karakteri bulamazsa otomatik bul
        if (characterHandler == null)
            characterHandler = FindObjectOfType<CharacterMaskHandler>();
    }

    void Update()
    {
        // 1. TAB AÇ
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ResetVisualsToCenter(); // Fýþkýrma efekti için sýfýrla

            if (wheelPanel != null) wheelPanel.SetActive(true);
            isWheelOpen = true;
            Time.timeScale = 0.2f;
        }

        // 2. TAB KAPAT
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (wheelPanel != null) wheelPanel.SetActive(false);
            isWheelOpen = false;
            Time.timeScale = 1f;

            // Maskeyi tak
            if (characterHandler != null) characterHandler.EquipMask(currentIndex);
        }

        // 3. SCROLL
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
            int previousIndex = currentIndex;

            if (scroll > 0) currentIndex--;
            else if (scroll < 0) currentIndex++;

            // Döngü
            if (currentIndex < 0) currentIndex = maskIcons.Length - 1;
            if (currentIndex >= maskIcons.Length) currentIndex = 0;

            // Eðer seçim deðiþtiyse SES ÇAL
            if (currentIndex != previousIndex)
            {
                PlayScrollSound();
            }
        }
    }

    void PlayScrollSound()
    {
        if (uiAudioSource != null && scrollSound != null)
        {
            // Pitch ile hafif ton deðiþimi (Robotik his)
            uiAudioSource.pitch = Random.Range(0.9f, 1.1f);

            // --- BURASI DEÐÝÞTÝ: SESÝ 3 KATINA ÇIKARDIK (3f) ---
            // Eðer hala az gelirse buradaki 3f'i 5f veya 10f yapabilirsin.
            uiAudioSource.PlayOneShot(scrollSound, 3f);
        }
    }

    void ResetVisualsToCenter()
    {
        // Animasyonun "yoktan var olmasý" için her þeyi merkeze topla
        for (int i = 0; i < maskIcons.Length; i++)
        {
            if (maskIcons[i] != null)
            {
                maskIcons[i].rectTransform.anchoredPosition = Vector2.zero;
                maskIcons[i].transform.localScale = Vector3.zero;
                Color c = maskIcons[i].color;
                c.a = 0f;
                maskIcons[i].color = c;
            }
        }
    }

    void UpdateMaskPositions()
    {
        for (int i = 0; i < maskIcons.Length; i++)
        {
            if (maskIcons[i] == null) continue;

            Vector2 targetPos = Vector2.zero;
            Vector3 targetScale = Vector3.zero;
            float targetAlpha = 0f;

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
            else // GÝZLÝ
            {
                targetPos = Vector2.zero;
                targetScale = Vector3.zero;
                targetAlpha = 0f;
            }

            // Animasyon (Lerp)
            RectTransform rect = maskIcons[i].rectTransform;
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * animationSpeed);
            rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);

            Color c = maskIcons[i].color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.unscaledDeltaTime * animationSpeed);

            // Seçili olan parlak beyaz, diðerleri hafif gri
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