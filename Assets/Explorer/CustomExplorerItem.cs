using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CustomExplorerItem : MonoBehaviour
{
    CustomExplorer explorer;

    public Image icon;
    public Text text;

    public string fullpath;

    public Color light, dark, selectedColor;
    public bool isDark;
    public bool selected;

    public void Setup(Sprite sprite, string fullpath, bool isDark, CustomExplorer explorer)
    {
        icon.sprite = sprite;
        this.fullpath = fullpath;
        text.text = Path.GetFileName(fullpath);
        this.isDark = isDark;
        GetComponent<Image>().color = isDark ? dark : light;

        this.explorer = explorer;
    }

    public void OnClick()
    {
        explorer.OnClick(this);
    }

    public void Select()
    {
        GetComponent<Image>().color = selectedColor;
        selected = true;
    }
    public void Deselect()
    {
        GetComponent<Image>().color = isDark ? dark : light;
        selected = false;
    }
}