using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectItem : MonoBehaviour
{
    public Animator animator { get { return GetComponent<Animator>(); } }

    public PublishUI publishUI;

    public ProjectListItem project;

    public bool isActionsShowed;
    public bool isClickable = true;

    public Image coverImage;
    public Text authorText, nameText;


    public void Refresh()
    {
        if (project == null)
        {
            return;
        }
        

        authorText.text = project.author;
        nameText.text = project.name;

        if (project.coverPath != "") coverImage.sprite = TheGreat.LoadSprite(project.coverPath);
    }


    public void OnClicked()
    {
        if (!isClickable) return;

        if (isActionsShowed)
        {
            animator.Play("HideHidden");
            // Resize content (120 is hidden layer height)
            transform.parent.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 120);

        }
        else
        {
            animator.Play("ShowHidden");
            transform.parent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 120);
        }
        isActionsShowed = !isActionsShowed;
    }

    public void OnPublishBtnClicked()
    {
        publishUI.OnPublishBtnClicked(this);
    }


    public void OnDelete()
    {
        Camera.main.GetComponent<MenuScript>().AskForDelete(project, OnDeleted);
        
    }
    public void OnDeleted(bool result)
    {
        if (result)
        {
            ProjectManager.DeleteProject(project);
            StartCoroutine(IOnDelete());
        }
    }
    IEnumerator IOnDelete()
    {
        animator.Play("Delete");
        isClickable = false;
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
        
    }


    public void OnOpen()
    {
        MenuScript.LoadEditor(project);
    }
}
