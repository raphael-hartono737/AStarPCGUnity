using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    { 
        // 2. Langsung pindah ke scene "OpeningVideo" untuk ditampilkan
        SceneManager.LoadScene("OpeningVideo");
    }

    public void QuitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();
    }
}