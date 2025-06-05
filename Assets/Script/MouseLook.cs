using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f; // Nilai sensitivitas yang lebih wajar
    [SerializeField] private float dampingFactor = 0.05f;

    [SerializeField] private float joystickSensitivity = 100f;
    [SerializeField] private float joystickDeadzone = 0.1f;

    [SerializeField] private Transform playerBody;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;

    private float xSmoothed = 0.0f;
    private float ySmoothed = 0.0f;

    private float xVel = 0.0f;
    private float yVel = 0.0f;

    bool _locked = false;

    public void SetLock(bool locked, Vector3 fwd)
    {
        if (_locked == locked) return;

        _locked = locked;

        if (locked)
        {
            transform.localRotation = Quaternion.identity;
            playerBody.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_locked) return;

        // Handle input dengan konsistensi Time.deltaTime
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float joyX = Input.GetAxis("RightStickHorizontal");
        float joyY = Input.GetAxis("RightStickVertical");

        // Terapkan deadzone untuk joystick
        if (Mathf.Abs(joyX) < joystickDeadzone) joyX = 0;
        if (Mathf.Abs(joyY) < joystickDeadzone) joyY = 0;

        // Gunakan Time.deltaTime yang sama untuk semua input
        yRotation += mouseX + (joyX * joystickSensitivity * Time.deltaTime);
        xRotation -= mouseY + (joyY * joystickSensitivity * Time.deltaTime);

        xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

        // Smoothing dengan pengecekan NaN
        xSmoothed = Mathf.SmoothDamp(xSmoothed, xRotation, ref xVel, dampingFactor);
        ySmoothed = Mathf.SmoothDamp(ySmoothed, yRotation, ref yVel, dampingFactor);

        // Pencegahan nilai NaN sebelum menerapkan rotasi
        if (!float.IsNaN(xSmoothed) && !float.IsNaN(ySmoothed))
        {
            transform.localRotation = Quaternion.Euler(xSmoothed, 0, 0);
            playerBody.rotation = Quaternion.Euler(0, ySmoothed, 0);
        }
        else
        {
            // Reset nilai jika terjadi NaN
            xSmoothed = 0f;
            ySmoothed = 0f;
            xRotation = 0f;
            yRotation = 0f;
        }
    }
}
