using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListItemUI : MonoBehaviour
{
    public ProjectListItem project;
    public DevelopMenu dm;

    public Text authorText, nameText;
    public Image coverImage;

    public void Setup(DevelopMenu dm, ProjectListItem item)
    {
        this.dm = dm;
        this.project = item;

        authorText.text = item.author;
        nameText.text = item.name;
        
        if(item.coverPath != "")
        {
            coverImage.sprite = TheGreat.LoadSprite(item.coverPath);
        }
    }

    public void OnClick()
    {
        dm.OnProjectBtnClicked(this);
    }

    public void OnAskForDelete()
    {
        dm.AskForDelete(this);
    }

    public void OnSettingsClick()
    {
        dm.OpenProjectSettings(this);
    }

    public void OnPublishClick()
    {
        dm.GetComponent<Publisher>().OnPublishBtnClicked(this);
    }
}
