using UnityEngine;

[CreateAssetMenu(fileName = "FrogMask", menuName = "Masks/Frog")]
public class FrogMask : MaskBase
{
    [Header("Kanca Ayarlarý")]
    public float maxDistance = 25f;    // Dil ne kadar uzaða gider?
    public float pullSpeed = 15f;      // Kendini çekme hýzý
    public LayerMask hookableLayers;   // Nerelere tutunabilir?

    public override void ActivateAbility(GameObject player)
    {
        Debug.Log("Kurbaða Ruhu: Dili fýrlat ve uç!");

        // Kamera merkezinden ýþýn at
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, hookableLayers))
        {
            // Arkadaþýnýn hareket scriptine þu bilgileri gönder:
            // "Kanka þu noktaya (hit.point) doðru karakteri uçur!"

            /* Örn: 
               player.GetComponent<PlayerMovement>().StartGrapple(hit.point, pullSpeed);
            */
        }
    }

    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("Dil geri çekildi.");
        // Çekim iþlemini durdur
    }
}