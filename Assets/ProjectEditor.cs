using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProjectEditor : MonoBehaviour
{
    public SecurityScript ss;

    public CustomExplorer customExplorer;

    public GameObject ProjectsView, CreateProjectView;

    public Transform explorer;
    public GameObject explorerPrefab;

    public string selectedProjectPath, selectedImportPath;

    public Button openBtn, deleteBtn, importBtn, exportBtn;

    public Image coverImg;

    private void Start()
    {
        Application.targetFrameRate = 60;

        CheckFolders();

        RefreshExplorer();
    }

    public void CheckFolders()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/AudioFiles")) Directory.CreateDirectory(Application.persistentDataPath + "/AudioFiles");
        if (!Directory.Exists(Application.persistentDataPath + "/Projects")) Directory.CreateDirectory(Application.persistentDataPath + "/Projects");
        //if (!Directory.Exists(Application.persistentDataPath + "/Export")) Directory.CreateDirectory(Application.persistentDataPath + "/Export");
        //if (!Directory.Exists(Application.persistentDataPath + "/Import")) Directory.CreateDirectory(Application.persistentDataPath + "/Import");
        if (!Directory.Exists(Application.persistentDataPath + "/TempFiles/AudioFiles/")) Directory.CreateDirectory(Application.persistentDataPath + "/TempFiles/AudioFiles/");
    }

    public void RefreshExplorer()
    {
        foreach(Transform child in explorer)
        {
            Destroy(child.gameObject);
        }

        string[] allFiles = Directory.GetFiles(Application.persistentDataPath + "/Projects");
        for (int i = 0; i < allFiles.Length; i++)
        {
            if(Path.GetExtension(allFiles[i]) == ".bsp")
            {
                GameObject item = Instantiate(explorerPrefab, explorer);
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -15 - (i * 30));
                //item.GetComponent<AudioFileItem>().type = AudioFileItem.Type.Prject;
                item.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(allFiles[i]);
                item.name = "selectProjectItem";
            }
        }
    }

    // =============================================================================================================
    // Создание нового проекта
    [Header("New Project")]
    public Text oggText;
    public Text coverText, authorText, nameText;
    public string selectedAudioFile, selectedImagePath;
    public GameObject badNameBtn;
    public Button createProjectBtn;

    public Project newProject;

    public void NewProject()
    {
        newProject = new Project();

        ProjectsView.SetActive(false);
        CreateProjectView.SetActive(true);
    }

    public void SelectOggFile()
    {
        customExplorer.Open(Application.persistentDataPath, OnOggSelected, ".ogg");
    }
    int mins, secs;
    public void OnOggSelected(string path)
    {
        if(path != customExplorer.code_cancel && path != customExplorer.code_noPermission)
        {
            oggText.text = Path.GetFileName(path);
            selectedAudioFile = path;

            AudioClip clip;
            using (WWW www = new WWW("file:///" + selectedAudioFile))
            {
                while (!www.isDone) { }
                clip = www.GetAudioClip();
            }
            mins = Mathf.FloorToInt(clip.length / 60f);
            secs = Mathf.FloorToInt(clip.length - mins * 60);

            string fullname = Path.GetFileNameWithoutExtension(path);
            string author = fullname.Split('-')[0];
            string name = fullname.Split('-')[1];

            authorText.text = author + " • " + mins + (secs < 10 ? ":0" + secs : ":" + secs);
            nameText.text = name;

            badNameBtn.SetActive(IsBadName(fullname));
            createProjectBtn.interactable = !IsBadName(fullname);

            newProject.author = author;
            newProject.name = name;
            newProject.mins = mins;
            newProject.secs = secs;
            newProject.audioFile = File.ReadAllBytes(selectedAudioFile);
        }
    }
    public bool IsBadName(string fullname)
    {
        if (!fullname.Contains("-")) return true;
        else if (fullname.Contains("_")) return true;
        else if (fullname.Split('-').Length > 2) return true;
        else if (fullname == "") return true;
        else return false;
    }
    public InputField renameAuthor, renameName;
    public void ApplyRename()
    {
        string author = renameAuthor.text;
        string name = renameName.text;

        authorText.text = author + " • " + mins + (secs < 10 ? ":0" + secs : ":" + secs);
        nameText.text = name;

        badNameBtn.SetActive(IsBadName(author + "-" + name));
        createProjectBtn.interactable = !IsBadName(author + "-" + name);

        newProject.author = author;
        newProject.name = name;
        newProject.mins = mins;
        newProject.secs = secs;
        newProject.audioFile = File.ReadAllBytes(selectedAudioFile);
    }

    public void SelectCoverFile()
    {
        customExplorer.Open(Application.persistentDataPath, OnCoverSelected, ".jpg");
    }
    public void OnCoverSelected(string path)
    {
        if (path != customExplorer.code_cancel && path != customExplorer.code_noPermission)
        {
            coverText.text = Path.GetFileName(path);
            selectedImagePath = path;
            OnCoverSelected();
        }
    }

    public void OnCoverSelected()
    {
        newProject.hasImage = true;

        byte[] imageBytes = File.ReadAllBytes(selectedImagePath);
        newProject.image = imageBytes;
        coverImg.sprite = LoadSprite(imageBytes);
        coverImg.color = Color.white;
    }
    public void AcceptCreatingProject()
    {
        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create(Application.persistentDataPath + "/Projects/" + newProject.author + "-" + newProject.name + ".bsp"))
        {
            binaryFormatter.Serialize(fileStream, newProject);
        }

        ProjectsView.SetActive(true);
        CreateProjectView.SetActive(false);

        RefreshExplorer();
    }
    public void ResetNewProjectUI()
    {
        authorText.text = "";
        nameText.text = "";
    }


    public void ExplorerItem_Clicked(GameObject item)
    {
        if(item.name == "selectProjectItem")
        {
            selectedProjectPath = item.GetComponentInChildren<Text>().text;
            openBtn.interactable = true;
            deleteBtn.interactable = true;
            exportBtn.interactable = true;
        }
        else if(item.name == "selectAudioFileItem")
        {
            selectedAudioFile = item.GetComponentInChildren<Text>().text;
        }
        else if(item.name == "selectImageItem")
        {
            selectedImagePath = item.GetComponentInChildren<Text>().text;
            OnCoverSelected();
        }
        else if(item.name == "importItem")
        {
            selectedImportPath = item.GetComponentInChildren<Text>().text;
        }
    }

    public void OpenProject()
    {
        LCData.loadingProjectName = selectedProjectPath;
        SceneManager.LoadScene("SongEditor");
    }

    public void DeleteProject()
    {
        File.Delete(Application.persistentDataPath + "/Projects/" + selectedProjectPath + ".bsp");
        selectedProjectPath = "";
        openBtn.interactable = false;
        deleteBtn.interactable = false;
        exportBtn.interactable = false;
        RefreshExplorer();
    }



    public void SaveProject(Project project)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create(Application.persistentDataPath + "/Projects/" + project.author + "-" + project.name + ".bsp"))
        {
            binaryFormatter.Serialize(fileStream, project);
        }
    }




    public Sprite LoadSprite(byte[] bytes)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(bytes);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);

        return NewSprite;
    }
    public Texture2D LoadTexture(byte[] bytes)
    {
        Texture2D Tex2D;
        Tex2D = new Texture2D(2, 2);
        if (Tex2D.LoadImage(bytes))
            return Tex2D;
        return null;
    }
}
