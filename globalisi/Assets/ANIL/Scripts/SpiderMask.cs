using UnityEngine;

[CreateAssetMenu(fileName = "SpiderMask", menuName = "Masks/SpiderMask")]
public class SpiderMask : MaskBase
{
    public override void ActivateAbility(GameObject player)
    {
        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canClimb = true;
            Debug.Log("<color=green>ÖRÜMCEK MASKESÝ AKTÝF:</color> " + maskName);
        }
    }

    public override void DeactivateAbility(GameObject player)
    {
        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.canClimb = false;
            pm.isClimbingNow = false;
            Debug.Log("<color=red>ÖRÜMCEK MASKESÝ DEVRE DIÞI</color>");
        }
    }
}