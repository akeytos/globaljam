using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Ayarlar")]
    public string itemName = "Maske";

    [Header("Parlama Ayarý")]
    // Burayý 'Color.yellow' yerine gri tonlarýnda bir renk yaptýk.
    // Böylece maske sapsarý boyanmaz, sadece kendi deseni karanlýkta parlar.
    // 0.4f þiddetidir. Daha çok parlasýn istersen 0.6f veya 0.8f yapabilirsin.
    public Color glowColor = new Color(0.4f, 0.4f, 0.4f);

    private Renderer myRenderer;
    private Color originalEmissionColor;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();

        // Ana objede renderer yoksa çocuklarýnda ara (bazen model alt objede olur)
        if (myRenderer == null)
            myRenderer = GetComponentInChildren<Renderer>();

        if (myRenderer != null)
        {
            // Orijinal emisyon rengini hafýzaya at (Genelde siyahtýr)
            if (myRenderer.material.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = myRenderer.material.GetColor("_EmissionColor");
            }
        }
    }

    public void OnLookEnter()
    {
        if (myRenderer != null)
        {
            // Materyalin Emission özelliðini aç ve belirlediðimiz yumuþak ýþýðý ver
            myRenderer.material.EnableKeyword("_EMISSION");
            myRenderer.material.SetColor("_EmissionColor", glowColor);
        }
    }

    public void OnLookExit()
    {
        if (myRenderer != null)
        {
            // Eski haline (sönük haline) döndür
            myRenderer.material.SetColor("_EmissionColor", originalEmissionColor);
        }
    }
}