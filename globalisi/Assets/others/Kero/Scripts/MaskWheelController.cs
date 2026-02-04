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

    [Header("Referanslar")]
    public PlayerMovement playerMovement;

    private int currentIndex = 0;
    private bool isOpen = false;

    void Start()
    {
        wheelPanel.SetActive(false);
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();

        // Ýkonlarý otomatik diz
        for (int i = 0; i < maskIcons.Length; i++)
        {
            if (i < playerMovement.allMasks.Count)
                maskIcons[i].sprite = playerMovement.allMasks[i].maskIcon;
            else maskIcons[i].gameObject.SetActive(false);
        }
    }

    // PlayerMovement'dan gelen emirle menüyü aç/kapat
    public void SetMenuState(bool state)
    {
        isOpen = state;
        wheelPanel.SetActive(state);

        if (state)
        {
            ResetVisualsToCenter();
            Time.timeScale = 0.2f; // Menü açýkken yavaþlatma
        }
        else
        {
            // KRÝTÝK DÜZELTME: Eðer aktif maske Baykuþ (OwlMask) ise zamaný 1 yapma!
            if (playerMovement.activeMask != null && playerMovement.activeMask is OwlMask)
            {
                // Baykuþ maskesi zaten ActivateAbility içinde kendi zamanýný (0.1f) ayarladý.
                // O yüzden menü kapanýrken zamaný tekrar 1f yaparak onu bozmuyoruz.
                Time.timeScale = (playerMovement.activeMask as OwlMask).timeSlowFactor;
            }
            else
            {
                Time.timeScale = 1f; // Diðer maskelerde veya maskesiz durumda zamaný normale döndür
            }
        }
    }

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
            if (scroll > 0) currentIndex--;
            else currentIndex++;

            if (currentIndex < 0) currentIndex = maskIcons.Length - 1;
            if (currentIndex >= maskIcons.Length) currentIndex = 0;
        }
    }

    void ResetVisualsToCenter()
    {
        foreach (var icon in maskIcons)
        {
            icon.rectTransform.anchoredPosition = Vector2.zero;
            icon.transform.localScale = Vector3.zero;
            Color c = icon.color; c.a = 0f; icon.color = c;
        }
    }

    void UpdateMaskPositions()
    {
        for (int i = 0; i < maskIcons.Length; i++)
        {
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