using UnityEngine;
using UnityEngine.SceneManagement;

public class Volcano : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Geçilecek sahnenin tam adı (Build Settings'e ekli olmalı)")]
    public string sahneAdi = "HAVALANDIRMA";

    [Header("Trigger Settings")]
    [Tooltip("Aynı anda birden fazla tetiklemeyi engeller")]
    public bool tekSeferlik = true;

    [Tooltip("İstersen küçük gecikme (0 = anında)")]
    public float gecikme = 0f;

    private bool tetiklendi = false;

    private void OnTriggerEnter(Collider other)
    {
        if (tetiklendi) return;

        if (other.CompareTag("Player"))
        {
            if (tekSeferlik) tetiklendi = true;

            if (gecikme <= 0f)
                SceneManager.LoadScene(sahneAdi);
            else
                Invoke(nameof(SahneyeGit), gecikme);
        }
    }

    private void SahneyeGit()
    {
        SceneManager.LoadScene(sahneAdi);
    }
}
