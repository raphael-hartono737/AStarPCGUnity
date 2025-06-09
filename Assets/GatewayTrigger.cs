using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatewayTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoxCollider gameObjectCollider;
    private Vector3 currentGameObject;
    private Vector3 calculatedposition;
    public static event System.Action OnGatewayDestroyed; 
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameObjectCollider = GetComponent<BoxCollider>();
        currentGameObject = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
        Vector3 offsetGO = new Vector3(0, 2, 0);
        calculatedposition = currentGameObject + offsetGO; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnGatewayDestroyed?.Invoke();
            gameObjectCollider.enabled = false;
            Instantiate(gameManager.keyItems[gameManager.keyItemsTreshold-1], calculatedposition, Quaternion.identity); 
            
        }
    }

}
