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

    // --- SENÝN SES AYARLARINI BURAYA EKLEDÝM ---
    [Header("Ses Ayarlarý")]
    public AudioSource uiAudioSource;
    public AudioClip scrollSound;

    [Header("Referanslar")]
    public PlayerMovement playerMovement;

    private int currentIndex = 0;
    private bool isOpen = false;

    void Start()
    {
        if (wheelPanel != null) wheelPanel.SetActive(false);

        if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();

        // Ýkonlarý otomatik diz (Arkadaþýnýn yazdýðý mantýk)
        for (int i = 0; i < maskIcons.Length; i++)
        {
            // Eðer playerda maske varsa ikonunu al, yoksa o kutuyu kapat
            if (playerMovement != null && i < playerMovement.allMasks.Count)
                maskIcons[i].sprite = playerMovement.allMasks[i].maskIcon;
            else
                if (maskIcons[i] != null) maskIcons[i].gameObject.SetActive(false);
        }
    }

    // PlayerMovement scripti burayý çaðýrarak menüyü açýyor (Arkadaþýnýn mantýðý)
    public void SetMenuState(bool state)
    {
        isOpen = state;
        if (wheelPanel != null) wheelPanel.SetActive(state);

        if (state) // Menü açýldýysa
        {
            ResetVisualsToCenter(); // Fýþkýrma efekti
            Time.timeScale = 0.2f;  // Slow motion
        }
        else // Menü kapandýysa
        {
            Time.timeScale = 1f;    // Normal zaman
        }
    }

    // PlayerMovement hangi maskeyi seçtiðimizi buradan öðreniyor
    public int GetCurrentIndex() => currentIndex;

    void Update()
    {
        if (isOpen)
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
            // --- SES ÝÇÝN DEÐÝÞÝKLÝK BURADA ---
            int previousIndex = currentIndex; // Eski konumu hatýrla

            if (scroll > 0) currentIndex--;
            else currentIndex++;

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

    // --- SENÝN SES FONKSÝYONUN ---
    void PlayScrollSound()
    {
        if (uiAudioSource != null && scrollSound != null)
        {
            uiAudioSource.pitch = Random.Range(0.9f, 1.1f);
            uiAudioSource.PlayOneShot(scrollSound, 3f); // Sesi 3 katýna çýkar
        }
    }

    void ResetVisualsToCenter()
    {
        foreach (var icon in maskIcons)
        {
            if (icon != null)
            {
                icon.rectTransform.anchoredPosition = Vector2.zero;
                icon.transform.localScale = Vector3.zero;
                Color c = icon.color; c.a = 0f; icon.color = c;
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

            if (i == currentIndex)
            {
                targetPos = Vector2.zero;
                targetScale = Vector3.one * centerScale;
                targetAlpha = 1f;
                maskIcons[i].transform.SetAsLastSibling();
            }
            else if (i == GetWrappedIndex(currentIndex - 1))
            {
                targetPos = new Vector2(0, verticalSpacing);
                targetScale = Vector3.one * sideScale;
                targetAlpha = 0.5f;
            }
            else if (i == GetWrappedIndex(currentIndex + 1))
            {
                targetPos = new Vector2(0, -verticalSpacing);
                targetScale = Vector3.one * sideScale;
                targetAlpha = 0.5f;
            }

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