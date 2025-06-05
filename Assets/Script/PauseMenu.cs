using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference ke UI panel pause menu

    private bool isPaused = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        // Cek jika tombol ESC ditekan
        if (Input.GetButtonDown("Cancel"))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // Fungsi untuk menjeda game
    public void PauseGame()
    {
        pauseMenuUI.SetActive(true); // Tampilkan menu pause
        Time.timeScale = 0f; // Hentikan waktu dalam game
        isPaused = true;

        // Optional: Lock cursor di tengah layar dan sembunyikan
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Fungsi untuk melanjutkan game
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Sembunyikan menu pause
        Time.timeScale = 1f; // Lanjutkan waktu dalam game
        isPaused = false;

        // Optional: Lock cursor di tengah layar dan sembunyikan
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Fungsi untuk kembali ke main menu
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Pastikan waktu kembali normal
        SceneManager.LoadSceneAsync(0); // Ganti "MainMenu" dengan nama scene utama Anda
    }
}