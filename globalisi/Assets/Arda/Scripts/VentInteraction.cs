using UnityEngine;
using UnityEngine.SceneManagement;

public class VentInteraction : MonoBehaviour
{
    public string sahneAdi = "HAVALANDIRMA"; // Geçilecek sahnenin tam adı
    public float etkilesimMesafesi = 3f;      // Oyuncunun ne kadar yakında olması gerektiği
    public KeyCode etkilesimTusu = KeyCode.E; // Kullanılacak tuş

    private bool oyuncuYakininda = false;

    void Update()
    {
        // Eğer oyuncu tetikleyici içindeyse ve E tuşuna basarsa
        if (oyuncuYakininda && Input.GetKeyDown(etkilesimTusu))
        {
            SceneManager.LoadScene(sahneAdi);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oyuncuYakininda = false;
        }
    }
}