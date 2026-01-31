using UnityEngine;

public class CharacterMaskHandler : MonoBehaviour
{
    [Header("3D Maske Modelleri")]
    // Karakterin kafasına yerleştirdiğin gerçek 3D maske objelerini buraya sırayla sürükle
    public GameObject[] realMaskModels;

    [Header("Kamera Ayarları")]
    public GameObject tpsCamera; // Şu anki dış kamera (Varsa)
    public GameObject fpsCamera; // Maskeyi takınca geçilecek kafa kamerası

    // Bu fonksiyonu Çark sisteminden çağıracağız
    public void EquipMask(int index)
    {
        // 1. Önce kafadaki tüm maskeleri gizle (Temizlik)
        foreach (var mask in realMaskModels)
        {
            if (mask != null) mask.SetActive(false);
        }

        // 2. Seçilen maskeyi aç
        if (index >= 0 && index < realMaskModels.Length)
        {
            if (realMaskModels[index] != null)
            {
                realMaskModels[index].SetActive(true);
                Debug.Log("Maske Takıldı: " + realMaskModels[index].name);
            }
        }

        // 3. FPS Moduna Geçiş Yap
        SwitchToFPSMode();
    }

    void SwitchToFPSMode()
    {
        // TPS kamerasını kapat, FPS'i aç
        if (tpsCamera != null) tpsCamera.SetActive(false);
        if (fpsCamera != null) fpsCamera.SetActive(true);

        Debug.Log("FPS Moduna Geçildi!");
    }
}