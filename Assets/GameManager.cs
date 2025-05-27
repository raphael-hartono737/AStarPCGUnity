using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Reference to the Player GameObject
    private GameObject player;
    // Reference to the RigidbodyFirstPersonController script
    private MonoBehaviour rigidbodyFirstPersonController;
    // Reference to the MainCamera's Canvas, Slider, and TextMeshPro
    public Slider countdownSlider;
    public TextMeshProUGUI generatingWorldText;
    public Canvas blackScreenCanvas;

    void Start()
    {
        // Find the Player GameObject by name or tag
        player = GameObject.Find("Player") ?? GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player GameObject not found!");
            return;
        }

        // Find the RigidbodyFirstPersonController script on the Player GameObject
        rigidbodyFirstPersonController = player.GetComponent<MonoBehaviour>();

        if (rigidbodyFirstPersonController == null)
        {
            Debug.LogError("RigidbodyFirstPersonController script not found on the Player GameObject!");
            return;
        }

        // Disable the RigidbodyFirstPersonController script
        rigidbodyFirstPersonController.enabled = false;

        // Ensure the black screen canvas is active
        if (blackScreenCanvas != null)
        {
            blackScreenCanvas.gameObject.SetActive(true);
        }

        // Initialize the slider
        if (countdownSlider != null)
        {
            countdownSlider.maxValue = 15f; // Set the maximum value of the slider to 15 seconds
            countdownSlider.value = 0f; // Start at 0
        }

        // Update the TextMeshPro text
        if (generatingWorldText != null)
        {
            generatingWorldText.text = "Generating World...";
        }

        // Start the countdown coroutine
        StartCoroutine(CountdownAndReactivate());
    }

    IEnumerator CountdownAndReactivate()
    {
        float currentTime = 0f; // Start at 0 seconds
        float duration = 15f; // Total countdown duration in seconds

        while (currentTime < duration)
        {
            // Update the slider value
            if (countdownSlider != null)
            {
                countdownSlider.value = currentTime;
            }

            // Wait for one second
            yield return new WaitForSeconds(1f);

            // Increment the current time
            currentTime++;
        }

        // Ensure the slider reaches the maximum value
        if (countdownSlider != null)
        {
            countdownSlider.value = duration;
        }

        // Reactivate the RigidbodyFirstPersonController script
        if (rigidbodyFirstPersonController != null)
        {
            rigidbodyFirstPersonController.enabled = true;
        }

        // Deactivate the black screen canvas
        if (blackScreenCanvas != null)
        {
            blackScreenCanvas.gameObject.SetActive(false);
        }
    }
}