using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; 
    private float startTimer;

    private void Start()
    {
        startTimer = Time.unscaledTime; 
    }

    private void Update()
    {
        float elapsedTime = Time.unscaledTime - startTimer;

    }
}
