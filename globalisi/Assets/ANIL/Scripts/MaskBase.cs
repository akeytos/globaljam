using UnityEngine;

public abstract class MaskBase : ScriptableObject
{
    public string maskName;
    public GameObject maskModel; // Elias'ýn kafasýna takýlacak model
    public Sprite maskIcon;      // UI Çarký için
    public float energyCost;     // Saniye baþýna veya kullaným baþýna enerji

    // Her maskenin kendine has yeteneðini burada tanýmlayacaðýz
    public abstract void ActivateAbility(GameObject player);
    public abstract void DeactivateAbility(GameObject player);
}