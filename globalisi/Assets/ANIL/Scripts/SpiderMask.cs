using UnityEngine;

[CreateAssetMenu(fileName = "SpiderMask", menuName = "Masks/Spider")]
public class SpiderMask : MaskBase
{
    public override void ActivateAbility(GameObject player)
    {
        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            // Týrmanma deðiþkeni yerine artýk canWeb'i aktif ediyoruz
            pm.canWeb = true;
            Debug.Log("<color=black>Örümcek Ruhu:</color> Aðlarýnla lüsid rüyada süzül...");
        }
    }

    public override void DeactivateAbility(GameObject player)
    {
        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canWeb = false;
            // Maskeyi çýkarýnca aktif bir að varsa kopsun
            pm.StopWebbing();
        }
    }
}