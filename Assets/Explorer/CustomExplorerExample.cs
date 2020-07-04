using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomExplorerExample : MonoBehaviour
{
    public CustomExplorer explorer;

    public void OnBtnClick()
    {
        explorer.Open(Application.persistentDataPath, ExplorerCallback);
    }

    public void ExplorerCallback(string path)
    {
        if (path == explorer.code_cancel)
        {
            Debug.Log("User canceled file selecting");
        }
        else if (path == explorer.code_noPermission)
        {
            Debug.Log("User have not permission");
        }
        else
        {
            Debug.Log("User selected: " + path);
        }
    }
}