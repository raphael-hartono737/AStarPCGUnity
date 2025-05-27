using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouseLook : MonoBehaviour
{
    public float mouseSensitivity = 2.0f;
    public float mouseVerticalClamp = 90.0f;

    private float verticalRotation = 0f;

    [SerializeField] private Transform playerBody;

    private void Start()
    {
        // Lock the cursor to the center of the screen  
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        MouseLook();
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Clamp vertical rotation to prevent flipping over  
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -mouseVerticalClamp, mouseVerticalClamp);

        // Apply horizontal rotation to the player  
        playerBody.Rotate(Vector3.up * mouseX);

        // Apply vertical rotation to the camera  
        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
