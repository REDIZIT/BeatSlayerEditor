using Assets.AccountManagement;
using GameNet.Operations;
using GameNet.WebAPI;
using ModernEditor.Popup;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class Publisher : MonoBehaviour
{
    public InputField nicknameFiled;
    public InputField emailField;
    public Text publishAuthor, publishName, audioText;
    public Image publishCover;
    public Transform processLocker;
    public Sprite uploadSprite, checkmarkSprite, errorSprite;
    public Text errorText;
    // Locker
    public Slider uploadProgressSlider;
    public Text uploadStatusText, uploadInfoText;
    public GameObject uploadOkButton;

    private Project selectedProject;
    private ProjectListItem selectedItem;

    public void OnPublishBtnClicked(ProjectListItemUI ui)
    {
        Debug.LogError("Not implemented");
        /*selectedProject = ProjectManager.LoadProject(ui.project);
        Debug.Log("On publish btn clicked");
        selectedItem = ui.project;*/
    }

    // On publish btn clicked
    public void PublishProject()
    {
        if (!IsProjectValid()) return;
    }
    public void UploadProject(Project project, ProjectListItem item, Action<int> onProgress, Action<OperationResult> onComplete)
    {
        selectedProject = project;
        selectedItem = item;

        if (!ValidateTrackname()) return;
        if (!ValidateProjectTime()) return;
        if (!ValidateCubesCount()) return;
        CorrectTrackname();


        selectedProject.creatorNick = AccountManager.LoggedAccount.Nick;
        ProjectManager.SaveProject(selectedProject);

        string trackname = selectedProject.author + "-" + selectedProject.name;

        // Pack project to .bsz and read bytes
        string compressedPath = Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + ".bsz";
        ProjectManager.CompressProject(selectedItem, compressedPath);

        byte[] bytes = File.ReadAllBytes(compressedPath);
        File.Delete(compressedPath);


        WebAPI.UploadProject(bytes, trackname, onProgress, onComplete);
    }
    private bool IsProjectValid()
    {
        errorText.text = "";

        if (nicknameFiled.text == "") { nicknameFiled.GetComponent<Image>().color = new Color(0.3922f, 0.044f, 0.044f); return false; }
        else { nicknameFiled.GetComponent<Image>().color = new Color(0.1176f, 0.1176f, 0.1176f); }

        if (emailField.text == "" || !emailField.text.Contains("@")) { emailField.GetComponent<Image>().color = new Color(0.3922f, 0.044f, 0.044f); return false; }
        else { emailField.GetComponent<Image>().color = new Color(0.1176f, 0.1176f, 0.1176f); }

        string trackname = selectedProject.author + "-" + selectedProject.name;

        // Check time
        AudioClip clip = null;
        string path = Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + Project.ToString(selectedProject.audioExtension);

        using (WWW www = new WWW("file:///" + path))
        {
            while (!www.isDone) { }
            clip = www.GetAudioClip();
        }

        if (clip.length < 60)
        {
            errorText.text = "Time should be more than minute";
            return false;
        }

        // Check cubes count
        if (selectedProject.beatCubeList.Count <= 30)
        {
            errorText.text = "Notes count should be more than 30 (you have only " + selectedProject.beatCubeList.Count + ")";
            return false;
        }

        return true;
    }
    private bool ValidateTrackname()
    {
        if (selectedProject.author.Contains("-") || selectedProject.name.Contains("-"))
        {
            PopupMessager.ShowError("Trackname is invalid", "Author and name of music shouldn't contain '-' symbol");
            return false;
        }

        return true;
    }
    private bool ValidateProjectTime()
    {
        int seconds = selectedProject.mins * 60 + selectedProject.secs;

        // 20 minutes
        if (seconds >= 60 * 20)
        {
            PopupMessager.ShowError("Music is too long",
                $"Your music lenght is {selectedProject.mins}:{(selectedProject.secs < 10 ? '0' + selectedProject.secs : selectedProject.secs)}. Max duration is 20 minutes.");
            return false;
        }

        return true;
    }
    private bool ValidateCubesCount()
    {
        bool isValid = selectedProject.difficulties.Count != 0;

        foreach (var difficulty in selectedProject.difficulties)
        {
            if (difficulty.beatCubeList.Count < 30)
            {
                PopupMessager.ShowError("Not enough cubes", "Minimum cubes/lines count is 30. Please make map, not demo :D");
                return false;
            }
        }
        

        return isValid;
    }
    private void CorrectTrackname()
    {
        selectedProject.author = selectedProject.author.Trim();
        selectedProject.name = selectedProject.name.Trim();
    }


    #region API

    public void Publish(AccountManager manager, string compressedProjectPath, string email, string time, Action<CI.HttpClient.HttpResponseMessage> callback, Action<CI.HttpClient.UploadStatusMessage> progress)
    {
        if (AccountManager.LoggedAccount == null)
        {
            callback(new CI.HttpClient.HttpResponseMessage() { Exception = new Exception("Not authorized") });
            return;
        }

        try
        {

            ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
            //ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;

            CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();

            byte[] buffer = File.ReadAllBytes(compressedProjectPath);

            var httpContent = new CI.HttpClient.MultipartFormDataContent();

            CI.HttpClient.ByteArrayContent content = new CI.HttpClient.ByteArrayContent(buffer, "multipart/form-data");
            httpContent.Add(content, "arr", Path.GetFileName(compressedProjectPath));
            httpContent.Add(new CI.HttpClient.StringContent(AccountManager.LoggedAccount.Nick), "nickname");
            httpContent.Add(new CI.HttpClient.StringContent(email), "email");
            httpContent.Add(new CI.HttpClient.StringContent(time), "audioTime");


            //if(url_publish.Contains("localhost")) Debug.LogError("UPLOADING TO LOCALHOST SERVER!!");
            //client.Post(new System.Uri(url_publish), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, callback, progress);
        }
        catch (Exception err)
        {
            callback(new CI.HttpClient.HttpResponseMessage() { Exception = err });
        }
    }

    #endregion
}
