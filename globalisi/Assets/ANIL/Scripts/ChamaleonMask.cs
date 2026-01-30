using UnityEngine;

[CreateAssetMenu(fileName = "ChameleonMask", menuName = "Masks/Chameleon")]
public class ChameleonMask : MaskBase
{
    [Header("Görünmezlik Ayarlarý")]
    public float transparencyAmount = 0.2f; // Ne kadar þeffaf olacak? (0 tam görünmez)
    public Color invisibleColor = new Color(1, 1, 1, 0.2f);

    public override void ActivateAbility(GameObject player)
    {
        Debug.Log("Bukalemun Ruhu: Kimse seni göremiyor...");

        // Karakterin tüm modellerini bul ve þeffaflaþtýr
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            // Shader'ýn "Transparent" modunda olduðundan emin olmalýsýn
            rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            rend.material.EnableKeyword("_ALPHABLEND_ON");
            rend.material.renderQueue = 3000;

            Color c = rend.material.color;
            c.a = transparencyAmount;
            rend.material.color = c;
        }

        // Düþmanlara "Ben artýk görünmezim" bilgisi gönderilmeli
        // player.tag = "InvisiblePlayer"; 
    }

    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("Bukalemun Ruhu: Görünür oldun!");

        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Color c = rend.material.color;
            c.a = 1.0f; // Tam görünür yap
            rend.material.color = c;
        }

        // player.tag = "Player";
    }
}