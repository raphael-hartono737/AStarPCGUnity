using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private GameObject foodGameObject;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Interacter")
        {
            Instantiate(foodGameObject);
            Destroy(this.gameObject); 
        }
    }
}

