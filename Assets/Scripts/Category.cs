using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Category : MonoBehaviour
{
    public string categoryName;                 // Category name (类别名称)
    public Location.Types type;                 // Type of the locations in this category (此类别下位置的类型)
    public TextMeshProUGUI header;              // UI text showing the category name (显示类别名称的文本)
    public GameObject body;                     // Container (父对象) — holds location buttons
    public NavigationManager navigationManager; // Reference to NavigationManager (位置列表来源)
    public GameObject locationButton;           // Prefab of a location button (位置按钮的预制体)

    void Start()
    {
        // If no reference set in Inspector, try to get the singleton instance (如果没手动指定，就自动获取单例)
        if (navigationManager == null)
            navigationManager = NavigationManager.Instance;

        // Loop through all locations and add those matching this category type (遍历所有位置，添加符合类型的)
        foreach (GameObject location in navigationManager.locations)
        {
            if ((location.GetComponent<Location>().type & type) != 0)
            {
                AddLocation(location);
            }
        }
    }

    // Add a location button to the category (在分类中添加一个位置按钮)
    private void AddLocation(GameObject location)
    {
        // Instantiate a new button inside "body" container (在 body 中生成按钮)
        LocationButton button = Instantiate(locationButton, body.transform)
            .GetComponent<LocationButton>();

        // Initialize button with location name (初始化按钮文字为位置名)
        button.Initialize(location.name);
    }

    // Makes sure the object name and header text in editor are correct (在编辑器中自动同步名称和标题)
    void OnValidate()
    {
        // Change GameObject name to category name (修改 GameObject 名称为类别名)
        gameObject.name = categoryName == null ? "Collapsable Window" : categoryName;

        // Set header UI text (设置 UI 标题文字)
        header.text = categoryName;
    }
}
