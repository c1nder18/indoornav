


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class HelperArrow : MonoBehaviour
{
    public Transform capsule;   // Invisible capsule to point at, it is always closer to destination than user
    ///public bool helperArrowEnabled = false; //missing


    private Transform target;   // Which object the arrow is supposed to point at
    private Image arrow;        // The image of the arrow
    private Camera cam;         // To get screen positions of objects
    
    private void Start()
    {
        arrow = GetComponent<Image>();
        cam = Camera.main;
        arrow.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Target setup
        //if (NavigationManager.Instance.robotToggle.isOn)
            //target = NavigationManager.Instance.robot.gameObject.transform;
        //if (NavigationManager.Instance.lineToggle.isOn)
            target = capsule;
        if (NavigationManager.Instance.arrowsToggle.isOn)
            target = capsule;
        else
        {
            target = null;
            arrow.enabled = false;
        }
        
        if (target == null) return;

        bool visible = IsVisible(target.gameObject);
        arrow.enabled = NavigationManager.Instance.HasDestination() && !visible;
        //arrow.enabled = helperArrowEnabled && NavigationManager.Instance.HasDestination() && !visible;


        if (!visible)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Vector3 screenPos = cam.WorldToScreenPoint(target.position); // screen position of the target

            // It can be behind the camera, so we get rid of that issue
            if (screenPos.z < 0)
            {
                screenPos *= -1;
            }

            // Direction from center to screen position
            Vector3 dir = (screenPos - screenCenter).normalized;

            float padTop = 650f;
            float padBottom = 700f;
            float padSide = 100f;
            Vector3 edgePos = screenCenter + dir * ((Screen.height / 2f) - padSide); // Push to the edge of screen
            // then clamp it to the "visible area"
            edgePos.x = Mathf.Clamp(edgePos.x, padSide, Screen.width - padSide);
            edgePos.y = Mathf.Clamp(edgePos.y, padBottom, Screen.height - padTop);
            arrow.rectTransform.position = edgePos;

            // Rotate arrow to point toward the target
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    // used from https://github.com/zloedi/offscreen_markers/blob/master/Assets/OffscreenMarker.cs
    private bool IsVisible(GameObject objectToCheck) {
        Plane [] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Renderer [] rends = objectToCheck.GetComponentsInChildren<Renderer>();
        foreach (var r in rends) {
            if (GeometryUtility.TestPlanesAABB(planes, r.bounds)) {
                return true;
            }
        }
        return false;
    }
}