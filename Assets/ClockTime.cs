using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClockTime : MonoBehaviour
{
    [SerializeField] private float currentTime = 0.0f;
    [SerializeField] private Light theSun; 

    private void Start()
    {
        StartCoroutine(NoonToNight());
    }

    private void Update()
    {
        if (currentTime == 10.0f)
        {
            StopCoroutine(NoonToNight());
            StartCoroutine(NightToNoon());
        }
        else if (currentTime == 0.0f)
        {
            StopCoroutine(NoonToNight());
            StartCoroutine(NightToNoon());
        }
    }

    private IEnumerator NoonToNight()
    {
        while (true)
        {
            currentTime += 1.0f; 
            theSun.intensity = Mathf.Clamp(theSun.intensity - 0.1f, 0f, 1f);
            yield return new WaitForSeconds(10);
        }

    }

    private IEnumerator NightToNoon()
    {
        while (true)
        {
            currentTime -= 1.0f;
            theSun.intensity = Mathf.Clamp(theSun.intensity + 0.1f, 0f, 1f);
            yield return new WaitForSeconds(10); 
        }
    }
}
