using UnityEngine;

[CreateAssetMenu(fileName = "DeerMask", menuName = "Masks/Deer")]
public class DeerMask : MaskBase
{
    [Header("Dash Ayarlarý")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;

    [Header("Zýplama Ayarlarý")]
    public int extraJumpCount = 1; // Çift zýplama için 1 ekstra hak

    public override void ActivateAbility(GameObject player)
    {
        Debug.Log("Geyik Ruhu Uyandý: Hýz ve Çeviklik Artýþý!");

        // Arkadaþýnýn yazdýðý hareket scriptine ulaþýp deðerleri deðiþtiriyoruz
        // Örn: player.GetComponent<PlayerMovement>().canDoubleJump = true;
    }

    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("Geyik Ruhu Dinlenmeye Çekildi.");
        // Deðerleri normale döndür
    }

    // Bu maske takýlýyken Dash yapmak için çaðrýlacak özel fonksiyon
    public void Dash(Rigidbody rb, Vector3 moveDirection)
    {
        rb.AddForce(moveDirection * dashForce, ForceMode.Impulse);
    }
}