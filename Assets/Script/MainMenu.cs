using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Memuat scene video (scene index 1)
        SceneManager.LoadSceneAsync("OpeningVideo");
    }

    public void QuitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();
    }
}