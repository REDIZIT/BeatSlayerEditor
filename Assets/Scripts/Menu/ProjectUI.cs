using Assets.SimpleLocalization;
using ProjectManagement;
using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LegacyEditor;
using UnityEngine;
using UnityEngine.UI;
using static NativeGallery;

public class ProjectUI : MonoBehaviour
{
    public DifficultUI difficultUI;

    public Transform projectListContent;
    public AudioSource asource;
    public Sprite defaultTrackCover;

    [Header("Misc")]
    public GameObject noProjectsYetText;
    public Text namingErrorText;

    [Header("Create new project")]
    public Image coverImage;
    public InputField nameField, authorField;
    public Text selectedAudioFileText;
    public Button playAudioBtn, createProjectBtn;

    [Header("Edit project")]
    public Text headerText;
    public Button editProjectBtn;

    public void ShowProjects()
    {
        foreach (Transform child in projectListContent) if (child.name != "ProjectItem") Destroy(child.gameObject);
        GameObject prefab = projectListContent.GetChild(0).gameObject;
        prefab.SetActive(true);


        ProjectListItem[] projects = ProjectManager.GetProjects();
        projectListContent.GetComponent<RectTransform>().sizeDelta = new Vector2(projectListContent.GetComponent<RectTransform>().sizeDelta.x, -8);
        foreach (var project in projects)
        {
            GameObject go = Instantiate(prefab, projectListContent);
            ProjectItem item = go.GetComponent<ProjectItem>();
            item.project = project;
            item.Refresh();

            projectListContent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 260 + 8);
        }

        noProjectsYetText.SetActive(projects.Length == 0);

        prefab.SetActive(false);
    }



    public Project selectedProject;
    string selectedProjectAuthor, selectedProjectName;
    public void OnCreateNewProjectBtnClicked()
    {
        createProjectBtn.gameObject.SetActive(true);
        editProjectBtn.gameObject.SetActive(false);
        namingErrorText.gameObject.SetActive(false);
        headerText.text = LocalizationManager.Localize("CreateNewProject");

        selectedProject = new Project();
    }
    public void OnEditProjectBtnClicked(ProjectItem item)
    {
        createProjectBtn.gameObject.SetActive(false);
        editProjectBtn.gameObject.SetActive(true);

        namingErrorText.gameObject.SetActive(false);
        if (ContainsSpecialSymbols(authorField.text)) { namingErrorText.gameObject.SetActive(true); }
        if (ContainsSpecialSymbols(nameField.text)) { namingErrorText.gameObject.SetActive(true); }

        headerText.text = LocalizationManager.Localize("ProjectSettings");



        selectedProject = ProjectManager.LoadProject(item.project, true);

        string trackname = selectedProject.author + "-" + selectedProject.name;
        authorField.text = selectedProject.author;
        nameField.text = selectedProject.name;
        selectedProjectAuthor = selectedProject.author;
        selectedProjectName = selectedProject.name;


        if (selectedProject.hasImage) coverImage.sprite = TheGreat.LoadSprite(Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + Project.ToString(selectedProject.imageExtension));

        StartCoroutine(TheGreat.LoadAudioClip(Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + Project.ToString(selectedProject.audioExtension), (AudioClip clip) =>
        {
            asource.clip = clip;
            playAudioBtn.interactable = true;

            CheckProject();
        }));
    }



    public void OnTracknameChange(InputField field)
    {
        if (field.name == nameField.name) selectedProjectName = field.text;
        else selectedProjectAuthor = field.text;

        CheckProject();
    }

    public void OnCoverClicked()
    {
        GetImageFromGallery(new MediaPickCallback(OnCoverSelected), "Select cover image");
    }
    public void OnCoverSelected(string path)
    {
        if(path == null || path == "") 
        { 
            Debug.LogError("Path is empty");
            coverImage.sprite = defaultTrackCover;
            selectedProject.hasImage = false;
            selectedProject.image = null;
        }
        coverImage.sprite = TheGreat.LoadSprite(path);
        selectedProject.image = File.ReadAllBytes(path);
        selectedProject.hasImage = true;
        selectedProject.imageExtension = Path.GetExtension(path) == ".png" ? Project.ImageExtension.Png : Project.ImageExtension.Jpeg;

        CheckProject();
    }

    public void OnSelectAudioFileClicked()
    {
        FileBrowser.SetFilters(false, new List<string>{
            ".mp3",
            ".ogg"
        });
        FileBrowser.ShowLoadDialog(new FileBrowser.OnSuccess(OnAudioFileSelected), new FileBrowser.OnCancel(() => { }), false, null, "Select audio file");
    }
    public void OnAudioFileSelected(string path)
    {
        selectedAudioFileText.text = "../" + Path.GetFileName(path);

        byte[] bytes = File.ReadAllBytes(path);
        selectedProject.audioFile = bytes;
        selectedProject.audioExtension = Path.GetExtension(path) == ".mp3" ? Project.AudioExtension.Mp3 : Project.AudioExtension.Ogg;

        StartCoroutine(TheGreat.LoadAudioClip(path, (AudioClip clip) =>
        {
            asource.clip = clip;
            playAudioBtn.interactable = true;

            float time = clip.length;
            int[] arr = TheGreat.SecondsToInts(time);
            selectedProject.mins = arr[0];
            selectedProject.secs = arr[1];

            string trackname = Path.GetFileNameWithoutExtension(path);
            selectedProjectAuthor = trackname.Split('-')[0].Trim();
            selectedProjectName = trackname.Split('-')[1].Trim();
            authorField.text = selectedProjectAuthor;
            nameField.text = selectedProjectName;

            CheckProject();
        }));
    }

    public void CheckProject()
    {
        bool isInteractable = true;

        if (selectedProjectName.Trim() == "" || selectedProjectAuthor.Trim() == "") isInteractable = false;
        if (selectedProject.audioFile == null || selectedProject.audioFile.Length == 0) isInteractable = false;
        if (ContainsSpecialSymbols(authorField.text)) { isInteractable = false; namingErrorText.gameObject.SetActive(true); }
        if (ContainsSpecialSymbols(nameField.text)) { isInteractable = false; namingErrorText.gameObject.SetActive(true); }

        createProjectBtn.interactable = isInteractable;
        editProjectBtn.interactable = isInteractable;
    }
    public void OnCreateProjectBtnClicked()
    {
        selectedProject.author = selectedProjectAuthor;
        selectedProject.name = selectedProjectName;
        ProjectManager.CreateProject(selectedProject);

        //ShowProjects();

        MenuScript.LoadEditor(new ProjectListItem() { author = selectedProject.author, name = selectedProject.name });
    }
    public void OnEditApplyBtnClicked()
    {
        if(selectedProject.author != selectedProjectAuthor || selectedProject.name != selectedProjectName)
        {
            ProjectManager.RenameProject(selectedProject, selectedProjectAuthor + "-" + selectedProjectName);
        }
        ProjectManager.SaveProject(selectedProject, true);

        ShowProjects();
    }
    public void OnChangeDifficultBtnClicked()
    {
        difficultUI.Show();
    }


    public void OnAudioBtnClicked(bool play)
    {
        if (play) asource.Play();
        else asource.Stop();
    }



    bool ContainsSpecialSymbols(string str)
    {
        List<char> exclude = new List<char>()
        {
            ' ', '(', ')', '_', '`', '\'', '\"', '/'
        };

        bool containsSpecialSymbol = str.Any(c => !char.IsLetterOrDigit(c) && !exclude.Contains(c));

        return containsSpecialSymbol;
    }
}
