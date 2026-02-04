using UnityEngine;

[CreateAssetMenu(fileName = "DeerMask", menuName = "Masks/Deer")]
public class DeerMask : MaskBase
{
    public override void ActivateAbility(GameObject player)
    {
        if (string.IsNullOrEmpty(maskName)) maskName = "Geyik";
        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.canDash = true; // DASH AKTÝF
    }

    public override void DeactivateAbility(GameObject player)
    {
        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.canDash = false; // DASH KAPALI
    }
}