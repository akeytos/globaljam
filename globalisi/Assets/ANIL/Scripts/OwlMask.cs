using UnityEngine;

[CreateAssetMenu(fileName = "OwlMask", menuName = "Masks/Owl")]
public class OwlMask : MaskBase
{
    [Range(0.01f, 1f)] public float timeSlowFactor = 0.3f;

    public override void ActivateAbility(GameObject player)
    {
        if (string.IsNullOrEmpty(maskName)) maskName = "Baykuþ";

        // ZAMANI DEÐÝÞTÝR
        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Debug.Log($"<color=yellow>ZAMAN BÜKÜLDÜ!</color> Mevcut Hýz: {Time.timeScale}");
    }

    public override void DeactivateAbility(GameObject player)
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        Debug.Log("<color=white>Zaman normale döndü.</color>");
    }
}