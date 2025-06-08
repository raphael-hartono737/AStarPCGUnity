using System.Collections;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.UI;


public class Waypoint : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float maxDistance = 2500f;
    [Range(0, 1)] public float viewDotThresh = 0.2f;
    Image sprite;
    Camera cam;

    //private void Awake()
    //{
        
    //}
    private void Start()
    {
        GameObject QuestPoint = GameObject.Find("QuestPoint");
        Transform questPointTransform = QuestPoint.transform;
        target = questPointTransform;
        cam = Camera.main;
        sprite = GetComponent<Image>();
    }


    private void Update()
    {
        if (Time.timeScale == 0f || target == null)
        {
            sprite.enabled = false;
            return;
        }

        Vector3 toTarget = target.position - cam.transform.position;
        float dist = toTarget.magnitude;

        // 1) is it in front?
        bool inFront = Vector3.Dot(cam.transform.forward, toTarget.normalized) > 0f;

        // 2) roughly within the camera’s forward cone?
        bool inViewCone = Vector3.Dot(cam.transform.forward, toTarget.normalized) > viewDotThresh;
        //    viewDotThresh = 1 → exactly forward, 
        //                  0 → any point on forward-facing hemisphere,
        //                 -1 → even behind.

        // 3) within distance?
        bool inRange = dist <= maxDistance;

        // enable only if BOTH in front AND (within cone OR within range)
        sprite.enabled = inFront && (inViewCone || inRange) && Time.timeScale != 0f;

        if (!sprite.enabled)
            return;

        // position it
        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        sprite.rectTransform.position = screenPos;

    }
}
