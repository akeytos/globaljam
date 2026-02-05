using UnityEngine;
using UnityEngine.EventSystems; // Hover kontrolü için þart

public class MenuButtonScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 initialScale;
    public float scaleFactor = 1.1f; // %10 büyüme
    public float transitionSpeed = 10f;
    private Vector3 targetScale;

    void Start()
    {
        initialScale = transform.localScale;
        targetScale = initialScale;
    }

    void Update()
    {
        // Lerp kullanarak yumuþak ama hýzlý bir geçiþ saðlar
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = initialScale * scaleFactor;
        // Ýstersen buraya ses ekleyebilirsin: AudioSource.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = initialScale;
    }
}