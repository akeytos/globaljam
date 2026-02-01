using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþi için þart

public class MainMenu : MonoBehaviour
{
    // Build Settings'deki sahne ismini buraya yazacaksýn
    public string firstLevelName = "GameScene";

    public void PlayGame()
    {
        // Sahneyi yüklerken asenkron yüklemek donmalarý engeller
        SceneManager.LoadScene(firstLevelName);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýldý!"); // Editörde çalýþtýðýný anlamak için
        Application.Quit();
    }
}