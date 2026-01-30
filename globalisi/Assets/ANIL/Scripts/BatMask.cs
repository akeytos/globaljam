using UnityEngine; // HATANIN ÇÖZÜMÜ BURASI: Bu satýr olmazsa hiçbir Unity komutu çalýþmaz.

[CreateAssetMenu(fileName = "BatMask", menuName = "Masks/Bat")]
public class BatMask : MaskBase
{
    // Yarasa yeteneði aktif olduðunda yapýlacaklar
    public override void ActivateAbility(GameObject player)
    {
        Debug.Log("Yarasa Vizyonu Aktif!");

        // Sahnedeki "Laser" tagine sahip tüm objeleri bul
        GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");

        foreach (GameObject laser in lasers)
        {
            // Lazerin görünürlüðünü aç
            if (laser.GetComponent<MeshRenderer>() != null)
            {
                laser.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    // Yarasa yeteneði kapandýðýnda yapýlacaklar
    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("Yarasa Vizyonu Devre Dýþý!");

        GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");

        foreach (GameObject laser in lasers)
        {
            // Lazerin görünürlüðünü kapat
            if (laser.GetComponent<MeshRenderer>() != null)
            {
                laser.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}