using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadingManager
{
    public static AsyncOperation gameLoadOp;

    public static void PreloadGameScene()
    {
        Debug.Log("[LoadingManager] Mulai preload scene 'Game' di background");
        gameLoadOp = SceneManager.LoadSceneAsync("Game");
        if (gameLoadOp == null)
        {
            Debug.LogError("[LoadingManager] Gagal melakukan LoadSceneAsync(\"Game\"). Periksa nama scene!");
            return;
        }
        gameLoadOp.allowSceneActivation = false;
        Debug.Log("[LoadingManager] allowSceneActivation = false; menunggu sampai video selesai");
    }

    public static void ActivateGameScene()
    {
        if (gameLoadOp != null)
        {
            Debug.Log("[LoadingManager] Mengaktifkan scene 'Game'!");
            gameLoadOp.allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("[LoadingManager] gameLoadOp masih null. Pastikan PreloadGameScene() terpanggil.");
        }
    }
}