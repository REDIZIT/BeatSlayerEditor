using Assets.AccountManagement;
using Assets.SimpleLocalization;
using GameNet.Operations;
using ModernEditor.Popup;
using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class PublishUI : MonoBehaviour
{
    public Publisher publisher;
    public AccountManager manager;

    public GameObject pleaseAuth;

    [Header("Overlay")]
    public GameObject publishPan;
    public Image coverImage;
    public Text authorText, nameText;
    public Slider progressBar;
    public Text progressLabel;
    public Button publishBtn, cancelBtn;

    [Header("Notifies")]
    public GameObject resetAfterUploadOverlay;

    private Project selectedProject;
    private ProjectListItem selectedItem;


    public const string url_getMaps = "http://www.bsserver.tk/Database/GetMap?trackname={0}&nick={1}";
    
    
    
    
    
    private void Start()
    {
        manager = GetComponent<MenuScript>().accountUI.manager;
    }




    /// <summary>
    /// On project item publish button click
    /// </summary>
    public void OnPublishBtnClicked(ProjectItem item)
    {
        if(AccountManager.LoggedAccount == null)
        {
            pleaseAuth.SetActive(true);
            return;
        }

        selectedItem = item.project;

        ShowOverlay(item);

        
        MapInfo info = GetMapInfo(selectedProject.author + "-" + selectedProject.name, AccountManager.LoggedAccount.Nick);
        if(info != null)
        {
            if (info.granted)
            {
                resetAfterUploadOverlay.SetActive(true);
            }
        }
    }
    /// <summary>
    /// On overlay publish button click
    /// </summary>
    public void OnPublishBtnClick()
    {
        publishBtn.interactable = false;
        cancelBtn.interactable = false;
        publisher.UploadProject(selectedProject, selectedItem, OnUploadProgress, OnUploadComplete);
    }



    private void ShowOverlay(ProjectItem item)
    {
        publishPan.SetActive(true);
        progressBar.gameObject.SetActive(false);
        progressBar.value = 0;
        progressLabel.text = "Waiting";
        publishBtn.interactable = true;
        cancelBtn.interactable = true;


        try
        {
            selectedProject = ProjectManager.LoadProject(item.project, true);
        }
        catch(Exception err)
        {
            PopupMessager.ShowError("File is corrupted!", "Contact with us by discord, we will try to restore it!");
            Debug.LogError("On publish map error\n" + err);
            publishPan.SetActive(false);
            return;
        }
        

        authorText.text = selectedProject.author;
        nameText.text = selectedProject.name;

        if (selectedProject.hasImage) coverImage.sprite = TheGreat.LoadSprite(selectedProject.image);
    }



    public void OnUploadProgress(int progess)
    {
        progressBar.gameObject.SetActive(true);
        progressLabel.text = $"Uploading ({progess}%)";
        progressBar.value = progess;
    }
    public void OnUploadComplete(OperationResult message)
    {
        Debug.Log("OnUploadComplete\n" + JsonConvert.SerializeObject(message));

        progressLabel.text = "Uploaded";
        progressBar.value = 100;

        if(message.state == OperationResult.State.Success)
        {
            PopupMessager.ShowSuccess("Project published!",
                "Your map is available in game now. You also can request approve status in 'Published maps' section if you want to have leaderboard on your map");
            publishPan.SetActive(false);
        }
        else
        {
            PopupMessager.ShowError("Something went wrong", message.message);
        }
    }
    
    
    /*public void OnDoPublishBtnClicked()
    {
        if (!emailField.text.Contains("@")) return;


        string trackname = selectedProject.author + "-" + selectedProject.name;

        string compressedProjectPath = Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + ".bsz";

        selectedProject.creatorNick = manager.LoggedAccount.Nick;

        ProjectManager.CompressProject(selectedProject, compressedProjectPath);



        publishPan.SetActive(false);
        progressOverlay.SetActive(true);
        stateText.text = "Waiting";


        publisher.Publish(manager, compressedProjectPath, emailField.text, selectedProject.mins + ":" + selectedProject.secs, OnPublishSuccess, OnPublishProgress);
    }*/
    /*public void OnPublishSuccess(CI.HttpClient.HttpResponseMessage msg)
    {
        if(msg.Exception != null)
        {
            Debug.LogError("Publish exception: " + msg.Exception.Message);
            return;
        }


        progressOverlay.SetActive(false);
        endOverlay.SetActive(true);


        string response = msg.ReadAsString();
        if(response.ToLower() != "success")
        {
            Debug.LogError("Publish error: " + response);

            endStateText.text = LocalizationManager.Localize("Error!");
            endStateText.color = new Color32(200, 50, 0, 255);
            endMessageText.text = response;
        }
        else
        {
            endStateText.text = LocalizationManager.Localize("Success!");
            endStateText.color = new Color32(0, 178, 30, 255);
            endMessageText.text = LocalizationManager.Localize("PublishSuccessMessage");

        }
    }
    public void OnPublishProgress(CI.HttpClient.UploadStatusMessage msg)
    {
        progressBar.value = msg.PercentageComplete;
        stateText.text = "Uploading..";
        percentsText.text = msg.PercentageComplete + "%";
    }




    public void CheckInteractable()
    {
        doPublishBtn.interactable = emailField.text.Contains("@");

        SettingsManager.settings.email = emailField.text;
        SettingsManager.Save();
    }
    */
    private MapInfo GetMapInfo(string trackname, string nick)
    {
        WebClient c = new WebClient();
        string url = string.Format(url_getMaps, trackname, nick);
        string response = c.DownloadString(url);

        return JsonConvert.DeserializeObject<MapInfo>(response);
    }
    
}