using UnityEngine;
using System.Collections.Generic;

public class MaskManager : MonoBehaviour
{
    [Header("Maske Listesi")]
    public List<MaskBase> allMasks; // Oluþturduðun 6 maske asset'ini buraya sürükle

    [Header("Durum Bilgileri")]
    public int selectedIndex = 0;   // Þu an tekerlekle seçilen (önizlenen) maske
    public MaskBase activeMask;    // Þu an Elias'ýn yüzünde takýlý olan maske
    private bool isMaskEquipped = false;

    void Update()
    {
        HandleScrollInput();   // Fare tekerleði kontrolü
        HandleEquipInput();    // 'E' tuþu kontrolü
    }

    // 1. ADIM: Maskeler Arasýnda Gezinti (Scroll)
    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Tekerleði çevirdikçe listede dön (Wrap-around)
            if (scroll > 0f) selectedIndex = (selectedIndex + 1) % allMasks.Count;
            else selectedIndex = (selectedIndex - 1 + allMasks.Count) % allMasks.Count;

            // Arkadaþýnýn UI kodunu buradan tetikleyebilirsin
            Debug.Log("Önizlenen Maske: " + allMasks[selectedIndex].maskName);
            UpdateUIPrompt();
        }
    }

    // 2. ADIM: Maskeyi Takma/Çýkarma (E Tuþu)
    void HandleEquipInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Zaten bir maske takýlýysa ve ayný maskeyi tekrar takmaya çalýþmýyorsak
            if (isMaskEquipped)
            {
                UnequipCurrentMask();

                // Eðer farklý bir maske seçiliyse onu tak, deðilse insan kal
                if (activeMask != allMasks[selectedIndex])
                {
                    EquipNewMask(allMasks[selectedIndex]);
                }
            }
            else
            {
                EquipNewMask(allMasks[selectedIndex]);
            }
        }
    }

    void EquipNewMask(MaskBase newMask)
    {
        activeMask = newMask;
        activeMask.ActivateAbility(gameObject); // Maskenin yeteneðini aç
        isMaskEquipped = true;

        Debug.Log(activeMask.maskName + " TAKILDI. FPS Moduna Geçiliyor...");

        // --- ARKADAÞINA NOT: Kamera geçiþ kodunu buraya baðla ---
        // CameraManager.Instance.SetFPS(true);
    }

    void UnequipCurrentMask()
    {
        if (activeMask != null)
        {
            activeMask.DeactivateAbility(gameObject); // Maskenin yeteneðini kapat
            Debug.Log(activeMask.maskName + " ÇIKARILDI. TPS Moduna Dönülüyor...");
        }

        activeMask = null;
        isMaskEquipped = false;

        // --- ARKADAÞINA NOT: Kamera geçiþ kodunu buraya baðla ---
        // CameraManager.Instance.SetFPS(false);
    }

    void UpdateUIPrompt()
    {
        // Buraya arkadaþýnýn UI'da seçili maskeyi parlatacaðý kodu ekleyeceksin
    }
}