using System;
using System.Collections;
using System.Collections.Generic;
using Immersal.Samples.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Location : MonoBehaviour
{
    // Location categories (位置类型枚举，可组合使用)
    [Flags]
    public enum Types
    {
        NONE = 0,
        ROOM = 1 << 0,           // 房间
        WC = 1 << 1,             // 卫生间
        GROUND_FLOOR = 1 << 2,   // 一层
        FLOOR_1 = 1 << 3,        // 二层
        FLOOR_2 = 1 << 4,        // 三层
        FLOOR_3 = 1 << 5,
        FLOOR_4 = 1 << 6,
        FLOOR_5 = 1 << 7,
        FLOOR_6 = 1 << 8,
        Lift = 1 << 9,           // 电梯
    };

    public string _name;                            // Location name (位置名称)
    public TextMeshPro label;                       // Door label text (门牌显示文字)
    public Types type;                              // Location type(s) (位置类型，可多选)
    public UnityEvent<GameObject> locationChange;   // Event when player enters location (玩家进入位置事件)
    public IsNavigationTarget navigationTarget;     // Navigation target (导航终点标记)
    public GameObject door;                         // Door prefab (门预制体)

    private bool _hasDoor;                          // Whether the location currently has a door (是否有门)

    // Whether this location is the current destination (当前是否是导航目的地)
    public bool isDestination
    {
        get => _hasDoor;
        set
        {
            _hasDoor = value;
            // Show door only if it's a ROOM or WC type and isDestination is true
            door.SetActive(((type & Types.ROOM) != 0 || (type & Types.WC) != 0) && _hasDoor);
        }
    }

    // Auto-update GameObject name in the editor and set door visibility (编辑器中自动命名和门显示)
    void OnValidate()
    {
        gameObject.name = _name;

        // Default door visibility
        door.SetActive((type & Types.ROOM) != 0 || (type & Types.WC) != 0);

        // WC is shown as "WC", otherwise use the location name
        label.text = (type & Types.WC) != 0 ? "WC" : _name;
    }

    private void Start()
    {
        // Initialize label text and ensure destination flag is false
        label.text = (type & Types.WC) != 0 ? "WC" : _name;
        isDestination = false;
    }

    // Triggered when player enters this location (玩家进入该位置时触发)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("EditorOnly"))
        {
            locationChange.Invoke(gameObject);
        }
    }
}
