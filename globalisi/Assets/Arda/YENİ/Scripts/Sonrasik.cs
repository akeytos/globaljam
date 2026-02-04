using UnityEngine;

public class TimeDebugger : MonoBehaviour
{
    void Update()
    {
        // Eðer zaman 1'e geri dönüyorsa konsolda anýnda görürsün
        if (Time.timeScale > 0.2f && Input.GetKey(KeyCode.Space))
            Debug.LogWarning("DÝKKAT: Zaman birisi tarafýndan tekrar hýzlandýrýldý! Mevcut: " + Time.timeScale);
    }
}