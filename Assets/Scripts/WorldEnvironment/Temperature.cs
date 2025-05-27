using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temperature : MonoBehaviour
{
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private Player playerGO; 
    [SerializeField] public float temperature = 30.0f;
    public float maxTemperature = 300.0f;
    public float minTemperature = -100.0f;


    void Update()
    {
        CheckEnvironment(); 
    }

    private void CheckEnvironment()
    {

    } 

    private void DecreaseHealth()
    {
        if (temperature >= maxTemperature || temperature <= minTemperature)
        {
            
        }
    }

}
