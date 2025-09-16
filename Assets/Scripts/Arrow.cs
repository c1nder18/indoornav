using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Main camera object (主摄像机对象) — used to measure distance between arrow and player
    private GameObject camera; // To calculate distance from arrow to user

    // Settings — arrow scaling rule (箭头缩放规则)
    public float referenceDistance = 1f; // From what distance does the arrow begin to shrink (开始缩小的参考距离)
    public float maxScale = 0.1f;         // Default and maximum scale of the arrow (箭头的最大缩放比例)
    public float minScale = 0.0f;         // Minimum scale (箭头最小缩放比例)

    void Start()
    {
        // Get the main camera in the scene (获取场景中的主摄像机)
        camera = Camera.main?.gameObject;
    }

    void Update()
    {
        // If no camera found, stop (没有摄像机就不执行)
        if (camera == null) return;

        // Calculate distance between arrow and camera (计算箭头和摄像机的距离)
        float distance = Vector3.Distance(transform.position, camera.transform.position);

        // Scale calculation based on distance (根据距离计算缩放)
        float scaleFactor = referenceDistance / distance;  // smaller distance → bigger arrow (距离越小箭头越大)
        scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale); // Limit the scale (限制缩放范围)

        // Apply the scale to the arrow (应用缩放)
        transform.localScale = Vector3.one * scaleFactor;
    }
}
