using UnityEngine;
using UnityEngine.UI;

public class MaskVisionUI : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Image overlayImage; // Kenar Görseli
    public Image tintImage;    // YENÝ: Tam Ekran Renk Filtresi

    [Header("Ayarlar")]
    public float fadeSpeed = 5f; // Geçiþ hýzý

    private float targetAlpha = 0f; // Hedef görünürlük (0 veya 1)
    private Color activeTintColor = Color.clear; // O anki maskenin hedef rengi

    void Start()
    {
        // Baþlangýçta her þeyi gizle
        SetImageAlpha(overlayImage, 0f);
        SetImageAlpha(tintImage, 0f);
    }

    // Maske takýlýnca çaðrýlýr (Artýk renk de alýyor)
    public void ShowVision(Sprite maskSprite, Color tintColor)
    {
        if (maskSprite == null)
        {
            overlayImage.sprite = null; // Eðer sprite yoksa boþalt
        }
        else
        {
            overlayImage.sprite = maskSprite;
        }

        activeTintColor = tintColor; // Yeni rengi kaydet

        // Renk filtresinin RGB'sini hemen ayarla, Alpha'sý Update'te açýlacak
        if (tintImage != null)
        {
            Color c = tintImage.color;
            c.r = tintColor.r; c.g = tintColor.g; c.b = tintColor.b;
            tintImage.color = c;
        }

        targetAlpha = 1f; // Görünür yap
    }

    // Maske çýkarýlýnca çaðrýlýr
    public void HideVision()
    {
        targetAlpha = 0f; // Gizle
    }

    void Update()
    {
        // --- Yumuþak Geçiþ (Fade Effect) ---

        // 1. Mevcut Alpha deðerini hedefe doðru yumuþat
        float currentAlpha = overlayImage.color.a;
        float nextAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);

        // 2. Kenar görseline uygula
        SetImageAlpha(overlayImage, nextAlpha);

        // 3. Renk filtresine uygula (Kendi belirlediði maksimum alpha'ya kadar)
        if (tintImage != null)
        {
            Color c = tintImage.color;
            // Hedef alpha (0-1 arasý) ile maskenin kendi alpha'sýný çarpýyoruz
            c.a = nextAlpha * activeTintColor.a;
            tintImage.color = c;
        }
    }

    // Yardýmcý fonksiyon: Sadece alpha deðiþtirir
    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}