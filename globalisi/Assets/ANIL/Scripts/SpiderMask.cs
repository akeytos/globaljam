using UnityEngine;

[CreateAssetMenu(fileName = "SpiderMask", menuName = "Masks/Spider")]
public class SpiderMask : MaskBase
{
    [Header("Týrmanma Ayarlarý")]
    public float climbSpeed = 4f; // Týrmanma hýzý
    [Tooltip("Hangi yüzeylere týrmanýlabilir?")]
    public LayerMask climbableLayers; // Sadece "Wall" layer'ýna sahip objelere týrmanmak için.

    public override void ActivateAbility(GameObject player)
    {
        Debug.Log("Örümcek Ruhu Aktif: Yerçekimine meydan oku!");

        // BURASI ÇOK ÖNEMLÝ:
        // Arkadaþýnýn yazdýðý hareket kodunda bir "ClimbingState" (Týrmanma Durumu) olmalý.
        // Biz bu maskeyi takýnca o durumu tetikleyeceðiz.

        // Örnek kullaným (Arkadaþýna gösterebilirsin):
        // var movementScript = player.GetComponent<PlayerMovementV2>();
        // if (movementScript != null)
        // {
        //     movementScript.canClimb = true;
        //     movementScript.SetClimbStats(climbSpeed, climbableLayers);
        // }
    }

    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("Örümcek Ruhu Pasif.");
        // Týrmanma yeteneðini kapatýyoruz.
        // var movementScript = player.GetComponent<PlayerMovementV2>();
        // if (movementScript != null)
        // {
        //     movementScript.canClimb = false;
        //     movementScript.StopClimbingNow(); // Eðer duvardaysa düþür.
        // }
    }
}