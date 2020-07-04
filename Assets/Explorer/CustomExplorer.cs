using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class CustomExplorer : MonoBehaviour
{
    // ==[ UI ]=========================
    public Transform explorerContent;
    public GameObject explorerWindow;
    GameObject itemPrefab;
    public Button selectBtn;
    public Text extText, pathText;

    // ==[ Explorer Info ]=========================
    CustomExplorerItem selectedItem;
    [HideInInspector] public string ext = "*";
    [HideInInspector] public string currentPath;
    [Header("Icons")]
    public Sprite folderIcon;
    public Sprite fileIcon, audioIcon, imageIcon;

    // ==[ Callbacks ]=========================
    [HideInInspector] public string code_cancel = "[canceled]";
    [HideInInspector] public string code_noPermission = "[no permission]";

    private void Start()
    {
        transform.SetAsFirstSibling();
        explorerWindow.SetActive(false);
    }

    Action<string> callback;
    public void Open(string startDir, Action<string> callback, string ext = "*")
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            callback(code_noPermission);
            explorerWindow.SetActive(false);
            return;
        }

        currentPath = startDir;
        this.ext = ext;
        extText.text = "Select file (" + (ext == "*" ? "any" : ext) + ")";

        itemPrefab = explorerContent.GetChild(0).gameObject;
        itemPrefab.name = "ItemPrefab";
        itemPrefab.SetActive(false);

        this.callback = callback;

        explorerWindow.SetActive(true);

        transform.SetAsLastSibling();

        GoToFolder(currentPath);
    }

    
    public void OnClick(CustomExplorerItem item)
    {
        if(item.icon.sprite == folderIcon)
        {
            GoToFolder(item.fullpath);
        }
        else
        {
            if (!item.selected)
            {
                if(selectedItem != null) selectedItem.Deselect();
                selectedItem = item;
                selectedItem.Select();

                if(ext != "*")
                {
                    selectBtn.interactable = Path.GetExtension(item.fullpath) == ext;
                }
                else
                {
                    selectBtn.interactable = true;
                }
            }
        }
    }

    public void SelectClick()
    {
        callback(selectedItem.fullpath);
        explorerWindow.SetActive(false);
        transform.SetAsFirstSibling();
    }

    public void Cancel()
    {
        callback(code_cancel);
        explorerWindow.SetActive(false);
        transform.SetAsFirstSibling();
    }

    public void Up()
    {
        DirectoryInfo info = new DirectoryInfo(currentPath);
        try
        {
            GoToFolder(info.Parent.FullName);
        }
        catch
        {
            Debug.LogError("Wait, that's illegal");
        }
    }

    void GoToFolder(string path)
    {
        foreach(Transform item in explorerContent)
        {
            if(item.name != "ItemPrefab")
            {
                Destroy(item.gameObject);
            }
        }


        string[] folders = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        bool isDark = false;
        foreach(string folder in folders)
        {
            GameObject item = Instantiate(itemPrefab, explorerContent);
            item.SetActive(true);

            string[] splitted = folder.Split(@"\".ToCharArray()[0]);
            string folderName = splitted[splitted.Length - 1];
            item.GetComponent<CustomExplorerItem>().Setup(folderIcon, folder, isDark, this);

            isDark = !isDark;
        }

        foreach (string file in files)
        {
            GameObject item = Instantiate(itemPrefab, explorerContent);
            item.SetActive(true);

            string ext = Path.GetExtension(file);
            Sprite icon = ext == ".ogg" ? audioIcon : ext == ".jpg" ? imageIcon : fileIcon;
            item.GetComponent<CustomExplorerItem>().Setup(icon, file, isDark, this);

            isDark = !isDark;
        }

        explorerContent.GetComponent<RectTransform>().sizeDelta = new Vector2(explorerContent.GetComponent<RectTransform>().sizeDelta.x, (folders.Length + files.Length) * 100);

        currentPath = path;
        pathText.text = currentPath;
    }
}
