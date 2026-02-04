using UnityEngine;

[CreateAssetMenu(fileName = "ChameleonMask", menuName = "Masks/ChameleonMask")]
public class ChameleonMask : MaskBase
{
    [Header("Bukalemun Ayarlarý")]
    public float invisibleAlpha = 0.2f; // Ne kadar þeffaf olacak?
    public string invisibleLayer = "InvisiblePlayer"; // Düþmanlarýn görmediði katman

    public override void ActivateAbility(GameObject player)
    {
        if (string.IsNullOrEmpty(maskName)) maskName = "Bukalemun";

        var meshRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshRenderer != null)
        {
            // Materyalin rengini þeffaflaþtýr
            Color c = meshRenderer.material.color;
            c.a = invisibleAlpha;
            meshRenderer.material.color = c;
        }

        // Katmaný deðiþtir (Düþmanlar bu katmaný görmeyecek þekilde ayarlanmalý)
        player.layer = LayerMask.NameToLayer(invisibleLayer);
        Debug.Log("<color=cyan>BUKALEMUN MASKESÝ AKTÝF:</color> Görünmezlik moduna geçildi.");
    }

    public override void DeactivateAbility(GameObject player)
    {
        var meshRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshRenderer != null)
        {
            Color c = meshRenderer.material.color;
            c.a = 1.0f; // Tam görünür yap
            meshRenderer.material.color = c;
        }

        player.layer = LayerMask.NameToLayer("Player"); // Normal katmana dön
        Debug.Log("<color=white>BUKALEMUN MASKESÝ DEVRE DIÞI</color>");
    }
}