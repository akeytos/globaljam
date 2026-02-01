using UnityEngine;

[CreateAssetMenu(fileName = "FrogMask", menuName = "Masks/Frog")]
public class FrogMask : MaskBase
{
    public float frogJumpPower = 6f; // Kurbaða takýlýyken zýplama yüksekliði
    private float normalJumpPower;

    public override void ActivateAbility(GameObject player)
    {
        if (string.IsNullOrEmpty(maskName)) maskName = "Kurbaða";
        Debug.Log("<color=green>Kurbaða Ruhu:</color> Þimdi daha yükseðe!");

        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            normalJumpPower = pm.jumpHeight; // Eski deðeri sakla
            pm.jumpHeight = frogJumpPower;   // Yeni deðeri ata
        }
    }

    public override void DeactivateAbility(GameObject player)
    {
        Debug.Log("<color=white>Kurbaða Ruhu:</color> Bacaklar normale döndü.");

        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.jumpHeight = normalJumpPower; // Eski deðere geri dön
        }
    }
}