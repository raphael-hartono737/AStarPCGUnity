using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI; 

public class Cheats : MonoBehaviour
{
    [SerializeField] private bool cheatsActivated;
    [SerializeField] private RigidbodyFirstPersonController playerController;
    [SerializeField] private float baseSpeed;
    [SerializeField] private float baseJumping;
    [SerializeField] private GameObject cheatMenuPanel;
    [SerializeField] private MainRoadGenerator roadGen;

    [SerializeField] private Toggle speedHack; 
    [SerializeField] private Toggle jumpHack1Toggle;
    [SerializeField] private Toggle jumpHack2Toggle;
    private bool cheatMenuActivated;
    void OnEnable()
    {
        GameManager.OnGameManagerComplete += HandleCheatsAvailable;
    }

    void OnDisable()
    {
        GameManager.OnGameManagerComplete -= HandleCheatsAvailable;
    }

    private void HandleCheatsAvailable()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<RigidbodyFirstPersonController>();
        if (playerController != null)
        {
            cheatsActivated = true;
            baseSpeed = playerController.movementSettings.ForwardSpeed;
            baseJumping = playerController.movementSettings.JumpForce; 
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!cheatMenuActivated)
            {
                CheatMenuOn();
            }
            else
            {
                CheatMenuOff(); 
            }
        }
    }

    private void CheatMenuOn()
    {
        cheatMenuActivated = true;
        cheatMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CheatMenuOff()
    {
        cheatMenuActivated = false;
        cheatMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    public void SpeedHack(bool isActivated)
    {
        if (cheatsActivated && isActivated)
        {
            playerController.movementSettings.ForwardSpeed = baseSpeed * 10f;
        }
        else
        {
            playerController.movementSettings.ForwardSpeed = baseSpeed;
        }
    }

    public void JumpHack1(bool isActivated)
    {
        if (!cheatsActivated) return;

        if (isActivated)
        {
            jumpHack2Toggle.onValueChanged.RemoveAllListeners();
            jumpHack2Toggle.isOn = false;
            jumpHack2Toggle.onValueChanged.AddListener(JumpHack2);

            playerController.movementSettings.JumpForce = baseJumping * 2f;
        }
        else
        {
            playerController.movementSettings.JumpForce = baseJumping;
        }
    }

    public void JumpHack2(bool isActivated)
    {
        if (!cheatsActivated) return;

        if (isActivated)
        {
            jumpHack1Toggle.onValueChanged.RemoveAllListeners();
            jumpHack1Toggle.isOn = false;
            jumpHack1Toggle.onValueChanged.AddListener(JumpHack1);

            playerController.movementSettings.JumpForce = baseJumping * 5f;
        }
        else
        {
            playerController.movementSettings.JumpForce = baseJumping;
        }
    }

    public void UnstuckPlayer()
    {
        Transform portLocation = roadGen.selected.transform;
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos = portLocation.position + new Vector3(0f, 5f, 0f);

        if (cheatsActivated)
        {
            playerGO.transform.position = spawnPos;
        }
    }

}
