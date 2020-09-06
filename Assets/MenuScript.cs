using Assets.AccountManagement;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization;
using CoversManagement;
using LegacyEditor;
using ModernEditor.Popup;

public class MenuScript : MonoBehaviour
{
    public AccountUI accountUI;
    public LoadingManager loadingManager;
    public SettingsUI settingsUI;
    public DifficultUI difficultUI { get { return GetComponent<DifficultUI>(); } }
    
    public PublishedProjectUI publishedProjectUI { get { return GetComponent<PublishedProjectUI>(); } }
    public ProjectUI projectUI { get { return GetComponent<ProjectUI>(); } }

    private EditorUpdater updater;


    [Header("UI")]
    public Transform tabPan;
    public Text versionText;
    public Texture2D defaultIcon;

    [Header("Messaging")]
    public Transform askForDeletePan;
    public Vector3 direction;
    public float angle, degrees, targetDeg;
    public BeatCubeClass.SubType subtype;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        CheckFolders();
    }

    void Start()
    {
        settingsUI.Init();

        accountUI.Init();
        accountUI.LogIn();

        publishedProjectUI.manager = accountUI.manager;

        projectUI.ShowProjects();

        versionText.text = Application.version;

        updater = GetComponent<EditorUpdater>();
        updater.Check();

        CoversManager.DefaultTexture = defaultIcon;
    }


    public void CheckFolders()
    {
        if(!Directory.Exists(Application.persistentDataPath + "/data/account")) Directory.CreateDirectory(Application.persistentDataPath + "/data/account");
        if(!Directory.Exists(Application.persistentDataPath + "/Maps")) Directory.CreateDirectory(Application.persistentDataPath + "/Maps");
    }





    public void OpenTab(string name)
    {
        foreach (Transform child in tabPan) child.gameObject.SetActive(child.name == name);
    }



    Action<bool> deleteCallback;
    public void AskForDelete(ProjectListItem item, Action<bool> callback)
    {
        askForDeletePan.parent.gameObject.SetActive(true);
        askForDeletePan.GetChild(1).GetComponent<Text>().text = $"Delete \'{item.author + " - " + item.name}\'?";
        deleteCallback = callback;
    }
    public void OnDeleteResult(bool result)
    {
        deleteCallback(result);
        deleteCallback = null;
        askForDeletePan.parent.gameObject.SetActive(false);
    }

    public static void LoadEditor(ProjectListItem project)
    {
        LCData.project = ProjectManager.LoadProject(project);
        Camera.main.GetComponent<MenuScript>().loadingManager.LoadScene();
    }




    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }


    void CheckUrl(string url)
    {
        if (url.ToLower().Contains("localhost"))
        {
            Debug.LogError("EDITOR IS WORKING WITH LOCALHOST\n" + url);
        }
    }
}
