using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Edge : MonoBehaviour
{
    [Header("Locations")]
    public GameObject a;    // Location A (位置A)
    public GameObject b;    // Location B (位置B)
    [Space(10)]

    [Header("Text Directions")]
    [TextArea] public string fromAToB;  // Directions from location A to location B (A到B的导航文字)
    [TextArea] public string fromBToA;  // Directions from location B to location A (B到A的导航文字)
    [Space(10)]

    [Header("Path of Edge")]
    public GameObject arrow; // Arrow prefab (箭头预制体)
    [Range(0, 15)]
    [Tooltip("Number of arrows in one segment of the path.")]
    public int segmentCount; // 每段路径上的箭头数量
    [Tooltip("Order sensitive list of points creating the path.")]
    [ContextMenuItem("Add Point", "AddPoint")]
    public List<GameObject> points = new List<GameObject>(); // 路径点列表（有顺序）

    // Lists for forward and backward arrows
    private List<GameObject> _arrowsForward = new List<GameObject>(); // Forward path: A -> B
    private List<GameObject> _arrowsBackward = new List<GameObject>(); // Backward path: B -> A

    // Calculates interpolated point position between A and B (用于在A和B之间计算插值点位置)
    private Vector3 InterpolatedPointPosition()
    {
        return Vector3.Lerp(
            a?.transform.position ?? Vector3.zero,
            b?.transform.position ?? Vector3.zero,
            (float)points.Count / (float)segmentCount
        );
    }

    // Add a new point along the path (在路径中添加一个新点)
    public void AddPoint()
    {
        GameObject point = new GameObject("p" + points.Count);
        point.transform.parent = transform;
        point.transform.position = InterpolatedPointPosition();
        points.Add(point);
    }

    private void Start()
    {
        GenerateArrows(); // 创建箭头
        Hide();           // 初始时隐藏
    }

    // Generate arrows along the path (生成路径上的箭头)
    private void GenerateArrows()
    {
        // Forward arrows (A -> B)
        for (int i = 0; i < points.Count - 1; i++)
        {
            GameObject arrowForward = Instantiate(arrow, transform);
            Vector3 start = points[i].transform.position;
            Vector3 end = points[i + 1].transform.position;

            Vector3 direction = end - start;
            Quaternion rotation = Quaternion.LookRotation(-direction, Vector3.up);

            arrowForward.transform.position = Vector3.Lerp(start, end, 0.5f);
            arrowForward.transform.rotation = rotation;
            _arrowsForward.Add(arrowForward);
        }

        // Backward arrows (B -> A)
        for (int i = points.Count - 1; i > 0; i--)
        {
            GameObject arrowBackward = Instantiate(arrow, transform);
            Vector3 start = points[i].transform.position;
            Vector3 end = points[i - 1].transform.position;

            Vector3 direction = end - start;
            Quaternion rotation = Quaternion.LookRotation(-direction, Vector3.up);

            arrowBackward.transform.position = Vector3.Lerp(start, end, 0.5f);
            arrowBackward.transform.rotation = rotation;
            _arrowsBackward.Add(arrowBackward);
        }
    }

    // Show forward arrows (显示A->B箭头)
    public void ShowForward()
    {
        foreach (GameObject arrow in _arrowsForward)
        {
            arrow.SetActive(true);
        }
        b.GetComponent<Location>().navigationTarget.gameObject.SetActive(true);
    }

    // Show backward arrows (显示B->A箭头)
    public void ShowBackward()
    {
        foreach (GameObject arrow in _arrowsBackward)
        {
            arrow.SetActive(true);
        }
        a.GetComponent<Location>().navigationTarget.gameObject.SetActive(true);
    }

    // Hide all arrows and navigation targets (隐藏所有箭头和导航目标)
    public void Hide()
    {
        foreach (GameObject arrow in _arrowsForward)
        {
            arrow.SetActive(false);
        }
        foreach (GameObject arrow in _arrowsBackward)
        {
            arrow.SetActive(false);
        }
        a.GetComponent<Location>().navigationTarget.gameObject.SetActive(false);
        b.GetComponent<Location>().navigationTarget.gameObject.SetActive(false);
    }

    // Automatically set the edge's name in the Inspector (在Inspector中自动命名)
    void OnValidate()
    {
        gameObject.name = a.name + " <-> " + b.name;
    }
}
