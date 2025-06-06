using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Waypoint : MonoBehaviour
{

    public Transform target;
    Image sprite;
    Camera cam;

    private void Awake()
    {
        GameObject QuestPoint = GameObject.Find("QuestPoint");
        Transform questPointTransform = QuestPoint.transform;
        target = questPointTransform; 
    }
    private void Start()
    {
        cam = Camera.main;
        sprite = GetComponent<Image>();
    }


    private void Update()
    {
        if (!target) return;
        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        sprite.rectTransform.position = screenPos;
        sprite.enabled = (screenPos.z > 0);

        if (Time.timeScale == 0f)
        {
            sprite.enabled = false;
            return;
        }

    }
}
