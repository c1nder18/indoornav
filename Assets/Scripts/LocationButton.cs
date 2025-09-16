

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationButton : MonoBehaviour
{
    public TextMeshProUGUI label;

    public void Initialize(string name)
    {
        gameObject.name = name;
        label.text = name;
    }

    public void ChangeDestination(){
        NavigationManager.Instance.DestinationChanged(gameObject.name);
    }
}
