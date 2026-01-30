using UnityEngine;

[CreateAssetMenu(fileName = "OwlMask", menuName = "Masks/Owl")]
public class OwlMask : MaskBase
{
    [Header("Zaman Ayarlarý")]
    [Range(0.1f, 1f)]
    public float slowMotionFactor = 0.3f; // Zamaný %30'a düþürür

    public override void ActivateAbility(GameObject player)
    {
        Debug.Log("Baykuþ Ruhu: Zaman senin için yavaþlýyor...");

        Time.timeScale = slowMotionFactor;
        // Fiziðin akýcý kalmasý için gerekli:
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("Zaman normale döndü.");

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
}