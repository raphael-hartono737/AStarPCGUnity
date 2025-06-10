using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class AutoPlayVideo : MonoBehaviour
{
    [Header("Nama scene tujuan setelah video selesai")]
    [SerializeField] private string nextSceneName = "NextScene";

    private VideoPlayer vp;

    private void Awake()
    {
        vp = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (vp.clip != null)
        {
            // Subscribe ke event ketika video sudah selesai (loopPointReached)
            vp.loopPointReached += OnVideoFinished;
            vp.Play();
        }
        else
        {
            Debug.LogWarning("VideoPlayer belum punya clip! Assign VideoClip-nya dulu di Inspector.");
        }
    }

    // Callback ketika video telah mencapai akhir
    private void OnVideoFinished(VideoPlayer source)
    {
        // Ganti ke scene berikutnya
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("Nama scene tujuan belum di‐set pada AutoPlayVideo!");
    }
}