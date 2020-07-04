using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplorerItem : MonoBehaviour
{
    public string value;

    public void Click()
    {
        Camera.main.GetComponent<ProjectEditor>().ExplorerItem_Clicked(gameObject);

        foreach(Transform child in transform.parent)
        {
            if(child == transform)
            {
                GetComponent<Image>().color = new Color32(80, 144, 255, 255);
            }
            else
            {
                child.GetComponent<Image>().color = new Color32(51, 51, 51, 255);
            }
        }
    }
}
