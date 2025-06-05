using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Game"; // Nama scene setelah video

    void Start()
    {
        // Otomatis ambil komponen VideoPlayer jika tidak di-assign
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Play();
    }

    void Update()
    {
        // Skip video dengan menekan tombol apa saja
        if (Input.anyKeyDown)
        {
            videoPlayer.Stop();
            LoadNextScene();
        }
    }

    void EndReached(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}