using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Linq;
using IngameDebugConsole;
using SimpleFileBrowser;
using Assets.SimpleLocalization;
using System.Net;
using System.Xml.Serialization;
using ProjectManagement;

public class DevelopMenu : MonoBehaviour
{
    public DebugLogManager logManager;
    public AudioSource aSource;

    public Text versionText;
    public Dropdown langDropdown;

    private void Awake()
    {
        if (PlayerPrefs.GetString("Lang") == "")
        {
            //LocalizationManager.AutoLanguage();
            PlayerPrefs.SetString("Lang", LocalizationManager.Language);
        }
        else
        {
            LocalizationManager.Language = PlayerPrefs.GetString("Lang");
        }

        langDropdown.value = LocalizationManager.Language == "Russian" ? 0 : 1;
    }
    private void Start()
    {
        Application.targetFrameRate = 60;
        versionText.text = "v " + Application.version;

        CheckForFolders();
        CheckForUpdates();

        // Upgrading projects
        #region Upgrading projects

        foreach (string filepath in Directory.GetFiles(Application.persistentDataPath + "/Projects").Where(c => Path.GetExtension(c) == ".bsp"))
        {
            Debug.Log("[UPGRADER] Upgrading to v2 " + filepath);

            Project proj;

            BinaryFormatter bin = new BinaryFormatter();
            using (var stream = File.OpenRead(filepath))
            {
                proj = (Project)bin.Deserialize(stream);
            }

            TheGreat.SaveProject(proj);

            File.Move(filepath, filepath.Replace(".bsp", ".bsp_backup"));
        }

        foreach (string filepath in Directory.GetFiles(Application.persistentDataPath + "/Projects").Where(c => Path.GetExtension(c) == ".bsz"))
        {
            Debug.Log("[UPGRADER] Upgrading to v3 " + filepath);

            Project proj;
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using (var stream = File.OpenRead(filepath))
            {
                proj = (Project)xml.Deserialize(stream);
            }

            Debug.LogWarning("Is audio null? " + (proj.audioFile == null));

            ProjectManager.SaveProject(proj, true);

            File.Move(filepath, filepath.Replace(".bsz", ".bsz_backup"));
        }
        #endregion

        ShowProjects();

        HandleSettings();
    }

    public bool doExtract;
    private void Update()
    {
        //UpdateTest();
        if (doExtract)
        {
            doExtract = false;
            ExtractToOgg();
        }
    }

    // ========== Раздел говнокода =========== //
    //   [DANGER] [DANGER] [DANGER] [DANGER]   //
    // ======================================= //

    void CheckForFolders()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Projects")) Directory.CreateDirectory(Application.persistentDataPath + "/Projects");
        if (!Directory.Exists(Application.persistentDataPath + "/Maps")) Directory.CreateDirectory(Application.persistentDataPath + "/Maps");
    }

    #region Проверка обновлений

    public GameObject updateLocker;
    public Text updateText;
    void CheckForUpdates()
    {
        if(Application.internetReachability != NetworkReachability.NotReachable)
        {
            WebClient c = new WebClient();
            string url = "http://176.107.160.146/Builds/GetEditorVersion";

            c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs args) =>
            {
                string response = args.Result;
                if (isNewVer(Application.version, response))
                {
                    updateLocker.gameObject.SetActive(true);
                    updateText.text = string.Format(LocalizationManager.Localize("NewVersionText"), response, Application.version);
                }
            };

            c.DownloadStringAsync(new Uri(url));
        }
    }
    public bool isNewVer(string cur, string next)
    {
        string[] strChars = cur.Split('.');
        int[] strInts = new int[strChars.Length];
        for (int i = 0; i < strChars.Length; i++) strInts[i] = int.Parse(strChars[i]);

        strChars = next.Split('.');
        int[] strInts2 = new int[strChars.Length];
        for (int i = 0; i < strChars.Length; i++) strInts2[i] = int.Parse(strChars[i]);

        if (strInts.Length != strInts.Length) return true;
        for (int i = 0; i < strInts.Length; i++)
        {
            if (strInts2[i] > strInts[i]) return true;
            else if (strInts2[i] < strInts[i]) return false;
        }
        return false;
    }
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    #endregion


    public Project selectedProject;
    string selectedProjectTrackname;

    #region Отображение проектов
    [Header("Отображение проектов")]
    public Transform projectContent;
    public Project[] displayedProjects;
    public string[] displayedProjectsPathes;

    public void ShowProjects()
    {
        foreach (Transform child in projectContent) if (child.name != "Item") Destroy(child.gameObject);
        projectContent.GetChild(0).gameObject.SetActive(true);


        ProjectListItem[] projects = ProjectManager.GetProjects();


        for (int i = 0; i < projects.Length; i++)
        {
            Transform item = Instantiate(projectContent.GetChild(0).gameObject, projectContent).transform;

            item.name = "Item " + i;

            item.GetComponent<ProjectListItemUI>().Setup(this, projects[i]);

            //if (projects[i].coverPath != "" && TheGreat.GetCoverPath(projects[i].coverPath) != "")
            //{
            //    item.GetChild(0).GetComponent<Image>().sprite = TheGreat.LoadSprite(TheGreat.GetCoverPath(projects[i].coverPath));
            //}
            ////bool isFileDiff = project.author + "-" + project.name != Path.GetFileNameWithoutExtension(displayedProjectsPathes[i]);
            //item.GetChild(1).GetComponent<Text>().text = projects[i].name;
            //item.GetChild(2).GetComponent<Text>().text = projects[i].author;


        }

        projectContent.GetChild(0).gameObject.SetActive(false);

        float itemHeight = projectContent.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
        float spacing = projectContent.GetComponent<VerticalLayoutGroup>().spacing;
        float contentHeight = (itemHeight * projects.Length) + (spacing * projects.Length - 1);
        projectContent.GetComponent<RectTransform>().sizeDelta = new Vector2(projectContent.GetComponent<RectTransform>().sizeDelta.x, contentHeight);

        if (projects.Length == 0) projectContent.parent.GetChild(0).gameObject.SetActive(true);
    }

    public void OnProjectBtnClicked(ProjectListItemUI ui)
    {
        //int index = int.Parse(item.name.Replace("Item ", ""));
        //LCData.project = displayedProjects[index];
        LCData.project = ProjectManager.LoadProject(ui.project);
        SceneManager.LoadScene("Editor");
    }

    #endregion

    #region Создание нового проекта

    [Header("Создание проекта")]
    public Image n_coverImg;
    public InputField n_nameField, n_authorField;
    public Text n_musicFilePath, n_coverImagePath;
    public Button createProjectBtn, n_playMusicBtn;
    public Project newProject;

    public void CreateProject()
    {
        if (!CanCreateProject()) return;

        //TheGreat.SaveProject(newProject);
        LCData.project = newProject;
        ProjectManager.CreateProject(newProject);

        SceneManager.LoadScene("Editor");
    }

    bool CanCreateProject()
    {
        if (n_musicFilePath.text == "") return false;
        if (n_nameField.text == "") return false;
        if (n_authorField.text == "") return false;
        if (n_nameField.text.Contains("-")) return false;
        if (n_authorField.text.Contains("-")) return false;

        return true;
    }
    public void ProjectBtnCheck()
    {
        createProjectBtn.interactable = CanCreateProject();
    }

    public void N_OnSelectMusicBtnClicked()
    {
        SelectMusicFile(newProject, n_nameField, n_authorField, n_musicFilePath, n_playMusicBtn);
        ProjectBtnCheck();
    }

    Project selectMusicProject;
    InputField selectMusicName, selectMusicAuthor;
    Text selectMusicFileText;
    Button selectMusicPlayBtn;
    void SelectMusicFile(Project project, InputField nameField, InputField authorField, Text musicFileText, Button playBtn)
    {
        //string[] pathes = StandaloneFileBrowser.OpenFilePanel("Выбери музыку", "", "ogg", false);
        selectMusicProject = project;
        selectMusicName = nameField;
        selectMusicAuthor = authorField;
        selectMusicFileText = musicFileText;
        selectMusicPlayBtn = playBtn;
        //explorer.Open(Application.persistentDataPath, SelectMusicFileCallback, ".ogg");
        FileBrowser.Filter filter = new FileBrowser.Filter("Ogg file", ".ogg");
        FileBrowser.Filter filter2 = new FileBrowser.Filter("Mp3 file", ".mp3");
        FileBrowser.SetFilters(false, filter2, filter);
        FileBrowser.ShowLoadDialog(SelectMusicFileCallback, () => { });
    }

    public void SelectMusicFileCallback(string path)
    {
        //if (path == explorer.code_cancel || path == explorer.code_noPermission) return;

        string ext = Path.GetExtension(path);
        selectMusicProject.audioExtension = ext == ".ogg" ? Project.AudioExtension.Ogg : Project.AudioExtension.Mp3;
        Debug.Log("Project audio ext is " + (selectMusicProject.audioExtension == Project.AudioExtension.Ogg ? "Ogg" : "Mp3"));

        selectMusicProject.audioFile = File.ReadAllBytes(path);

        string[] track = Path.GetFileNameWithoutExtension(path).Replace(" - ", "-").Split('-');
        selectMusicAuthor.text = track[0];
        if (track.Length > 1) selectMusicName.text = track[1];

        selectMusicFileText.text = "Загрузка..";
        StartCoroutine(LoadAudioFile(path, selectMusicProject, selectMusicPlayBtn, selectMusicFileText));
        //LoadAudioFile(path, selectMusicProject, selectMusicPlayBtn, selectMusicFileText);
    }
    IEnumerator LoadAudioFile(string path, Project project, Button playBtn, Text musicFilePath)
    {
        playBtn.interactable = false;
        if (aSource.isPlaying) aSource.Stop();
        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            aSource.clip = www.GetAudioClip();
            int[] time = TheGreat.SecondsToInts(aSource.clip.length);
            //project.mins = Mathf.FloorToInt(aSource.clip.length / 60);
            //project.secs = Mathf.FloorToInt(aSource.clip.length - project.mins * 60);
            project.mins = time[0];
            project.secs = time[1];
            musicFilePath.text = path;
            playBtn.interactable = true;
        }
    }

    void ExtractToOgg()
    {
        //EditorUtility.ExtractOggFile(aSource.clip, Application.persistentDataPath + "/exported.ogg");
    }

    public void N_OnSelectCoverBtnClicked()
    {
        SelectCoverFile(newProject, n_coverImg, n_coverImagePath);
        ProjectBtnCheck();
    }

    Project selectCoverProject;
    Image selectCoverImage;
    Text selectCoverText;
    void SelectCoverFile(Project project, Image coverImage, Text coverImageText)
    {
        //string path = StandaloneFileBrowser.OpenFilePanel("Выбери обложку", "", "jpg", false)[0];
        //if (path == "") return;

        selectCoverProject = project;
        selectCoverImage = coverImage;
        selectCoverText = coverImageText;
        //explorer.Open(Application.persistentDataPath, SelectCoverFileCallback, ".jpg");
        FileBrowser.Filter filter = new FileBrowser.Filter("Audio file", ".jpg");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowLoadDialog(SelectCoverFileCallback, ()=> { });
    }
    void SelectCoverFileCallback(string path)
    {
        //if(path == explorer.code_cancel || path == explorer.code_noPermission) return;

        selectCoverText.text = path;
        selectCoverProject.hasImage = true;
        selectCoverProject.image = File.ReadAllBytes(path);
        selectCoverImage.sprite = TheGreat.LoadSprite(selectCoverProject.image);
    }

    public void N_OnTracknameChanged(InputField field)
    {
        TracknameChanged(newProject, field);
        ProjectBtnCheck();
    }
    void TracknameChanged(Project project, InputField field)
    {
        if (field.name == n_nameField.name)
        {
            //project.name = field.text;
            selectedProjectTrackname = selectedProjectTrackname.Split('-')[0] + "-" + field.text;
        }
        else if (field.name == n_authorField.name)
        {
            //project.author = field.text;
            selectedProjectTrackname = field.text + "-" + selectedProjectTrackname.Split('-')[1];
        }
    }
    public void N_PlayMusicBtnClicked()
    {
        PlayMusicBtnClicked(n_playMusicBtn);
    }
    void PlayMusicBtnClicked(Button playBtn)
    {
        if (playBtn.GetComponentInChildren<Text>().text == "Воспроизвести")
        {
            aSource.Play();
            playBtn.GetComponentInChildren<Text>().text = "Пауза";
        }
        else
        {
            aSource.Pause();
            playBtn.GetComponentInChildren<Text>().text = "Воспроизвести";
        }
    }

    #endregion

    #region Удаление проекта
    ProjectListItemUI deletingProject;
    public void AskForDelete(ProjectListItemUI ui)
    {
        //int index = int.Parse(item.name.Replace("Item ", ""));
        //deletingProject = displayedProjectsPathes[index];
        deletingProject = ui;
    }
    public void DeleteProject()
    {
        string folderPath = Application.persistentDataPath + "/Maps/" + deletingProject.project.author + "-" + deletingProject.project.name;
        Directory.Delete(folderPath, true);
        ShowProjects();
    }
    #endregion

    #region Настройки проекта

    [Header("Настройки проекта")]
    public Image s_coverImg;
    public InputField s_nameField, s_authorField;
    public Text s_musicFilePath, s_coverImagePath;
    public Button applyChangesBtn, s_playMusicBtn;
    public Dropdown audioExtensionDropdown;

    public void OpenProjectSettings(ProjectListItemUI ui)
    {
        //int index = int.Parse(item.name.Replace("Item ", ""));
        selectedProject = ProjectManager.LoadProject(ui.project);
        selectedProjectTrackname = selectedProject.author + "-" + selectedProject.name;


        s_nameField.text = selectedProject.name;
        s_authorField.text = selectedProject.author;
        s_musicFilePath.text = "[Конвертирован в байты]";
        audioExtensionDropdown.value = selectedProject.audioExtension == Project.AudioExtension.Mp3 ? 0 : 1;
        if (selectedProject.hasImage)
        {
            s_coverImagePath.text = "[Конвертирован в байты]";
            s_coverImg.sprite = TheGreat.LoadSprite(selectedProject.image);
        }
        StartCoroutine(LoadAudioClip(selectedProject, s_playMusicBtn));
    }
    public void ApplyProjectChanges()
    {
        if (!CanApplyChanges()) return;

        //var binaryFormatter = new BinaryFormatter();
        //using (var fileStream = File.Create(Application.persistentDataPath + "/Projects/" + selectedProject.author + "-" + selectedProject.name + ".bsz"))
        //{
        //    binaryFormatter.Serialize(fileStream, selectedProject);
        //}
        //TheGreat.SaveProject(selectedProject);
        if(selectedProjectTrackname != selectedProject.author + "-" + selectedProject.name)
        {
            ProjectManager.RenameProject(selectedProject, selectedProjectTrackname);
        }
        ProjectManager.SaveProject(selectedProject);

        ShowProjects();
    }

    bool CanApplyChanges()
    {
        if (s_musicFilePath.text == "") return false;
        if (s_nameField.text == "") return false;
        if (s_authorField.text == "") return false;
        if (s_nameField.text.Contains("-")) return false;
        if (s_authorField.text.Contains("-")) return false;

        return true;
    }

    public void ProjectChangesBtnCheck()
    {
        applyChangesBtn.interactable = CanApplyChanges();
    }

    public void S_OnSelectMusicBtnClicked()
    {
        SelectMusicFile(selectedProject, s_nameField, s_authorField, s_musicFilePath, s_playMusicBtn);
        ProjectChangesBtnCheck();
    }
    public void S_OnSelectCoverBtnClicked()
    {
        SelectCoverFile(selectedProject, s_coverImg, s_coverImagePath);
        ProjectChangesBtnCheck();
    }
    public void S_OnTracknameChanged(InputField field)
    {
        TracknameChanged(selectedProject, field);
        ProjectChangesBtnCheck();
    }
    public void S_PlayMusicBtnClicked()
    {
        PlayMusicBtnClicked(s_playMusicBtn);
        ProjectChangesBtnCheck();
    }
    public void OnAudioExtensionChanged()
    {
        selectedProject.audioExtension = audioExtensionDropdown.value == 0 ? Project.AudioExtension.Mp3 : Project.AudioExtension.Ogg;
        ProjectChangesBtnCheck();
    }
    public void ExtractAudioFile()
    {
        if (selectedProject.audioFile == null) Debug.LogError("Audio bytes is NULL");

        string path = Application.persistentDataPath + "/Projects/" + selectedProject.author + "-" + selectedProject.name + (selectedProject.audioExtension == Project.AudioExtension.Ogg ? ".ogg" : ".mp3");
        File.WriteAllBytes(path, selectedProject.audioFile);
    }


    #endregion

    #region Публикация проекта





    #endregion

    #region Вспомогательные методы

    IEnumerator LoadAudioClip(Project project, Button playBtn)
    {
        string trackname = project.author + "-" + project.name;
        string path = Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + Project.ToString(project.audioExtension);
        Debug.LogWarning(path);
        playBtn.interactable = false;

        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            aSource.clip = www.GetAudioClip();
        }

        playBtn.interactable = true;
    }
    IEnumerator LoadAudioClipLegacy(Project project, Button playBtn)
    {
        string path = Application.persistentDataPath + "/Projects/tempaudio." + (project.audioExtension == Project.AudioExtension.Mp3 ? "mp3" : "ogg");
        playBtn.interactable = false;

        File.WriteAllBytes(path, project.audioFile);
        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            aSource.clip = www.GetAudioClip();
        }

        playBtn.interactable = true;
        File.Delete(path);
    }

    public void OpenWebsite()
    {
        Application.OpenURL("http://176.107.160.146/Home/Download");
    }
    public void OpenMail()
    {
        Application.OpenURL("iv24032004@gmail.com");
    }

    #endregion

    #region Настройки
    [Header("Настройки")]
    public Transform settingsBody;
    bool isSettingsLocked;

    void HandleSettings()
    {
        SettingsManager.Load();
        isSettingsLocked = true;
        settingsBody.GetChild(0).GetChild(1).GetComponent<Dropdown>().value = SettingsManager.settings.usePostProcess ? 0 : 1;
        settingsBody.GetChild(1).GetChild(1).GetComponent<Dropdown>().value = SettingsManager.settings.useConsole ? 0 : 1;
        isSettingsLocked = false;

        logManager.gameObject.SetActive(SettingsManager.settings.useConsole);
    }
    public void OnSettingChange()
    {
        if (isSettingsLocked) return;
        SettingsManager.settings.usePostProcess = settingsBody.GetChild(0).GetChild(1).GetComponent<Dropdown>().value == 0;
        SettingsManager.settings.useConsole = settingsBody.GetChild(1).GetChild(1).GetComponent<Dropdown>().value == 0;
        SettingsManager.Save();

        logManager.gameObject.SetActive(SettingsManager.settings.useConsole);
    }
    public void OnLangChange(Dropdown change)
    {
        if (Time.time < 1) return;
        LocalizationManager.Language = change.value == 0 ? "Russian" : "English";
        PlayerPrefs.SetString("Lang", LocalizationManager.Language);
    }
    #endregion

    public void UpgradeProjects()
    {
        string[] maps = Directory.GetFiles(@"C:\Users\REDIZ\source\repos\BeatSlayerServer\BeatSlayerServer\UploadData").Where(c => c.Contains(".bsp")).ToArray();
        string[] configs = Directory.GetFiles(@"C:\Users\REDIZ\source\repos\BeatSlayerServer\BeatSlayerServer\UploadData").Where(c => c.Contains(".txt")).ToArray();

        for (int i = 0; i < maps.Length; i++)
        {
            Debug.Log("Upgrading " + maps[i]);

            string path = maps[i];
            string newPath = path.Replace(".bsp", ".bsz");

            string[] cfgLines = File.ReadAllLines(configs[i]);
            string nick = cfgLines[1].Split(':')[1].Trim();

            BinaryFormatter bin = new BinaryFormatter();
            Project proj;
            using (FileStream st = File.OpenRead(path))
            {
                proj = (Project)bin.Deserialize(st);
            }

            proj.creatorNick = nick;

            XmlSerializer xml = new XmlSerializer(typeof(Project));
            xml.Serialize(File.Create(newPath), proj);
        }
    }
}







public static class TheGreat
{
    public static Sprite LoadSprite(byte[] bytes)
    {
        Texture2D SpriteTexture = LoadTexture(bytes);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);

        return NewSprite;
    }
    public static Sprite LoadSprite(string path)
    {
        return LoadSprite(File.ReadAllBytes(path));
    }
    public static Texture2D LoadTexture(byte[] bytes)
    {
        Texture2D Tex2D;
        Tex2D = new Texture2D(2, 2);
        if (Tex2D.LoadImage(bytes))
            return Tex2D;
        return null;
    }
    public static string GetCoverPath(string trackname)
    {
        if (File.Exists(Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + ".jpg")) return Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + ".jpg";
        else if (File.Exists(Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + ".png")) return Application.persistentDataPath + "/Maps/" + trackname + "/" + trackname + ".png";
        else return "";
    }


    //public static Project LoadProject(string path)
    //{
        
    //}
    public static void SaveProject(Project project)
    {
        XmlSerializer xml = new XmlSerializer(typeof(Project));
        string path = Application.persistentDataPath + "/Projects/" + project.author + "-" + project.name + ".bsz";
        Stream stream = File.Create(path);
        xml.Serialize(stream, project);
        stream.Close();
    }

    public static IEnumerator GetAudioClip(Project project, AudioClip clip)
    {
        string path = Application.persistentDataPath + "/Projects/tempaudio." + (project.audioExtension == Project.AudioExtension.Mp3 ? "mp3" : "ogg");
        File.WriteAllBytes(path, project.audioFile);
        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            clip = www.GetAudioClip();
        }
        File.Delete(path);
    }
    public static IEnumerator LoadAudioClip(string path, Action<AudioClip> callback)
    {
        AudioClip clip = null;
        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            clip = www.GetAudioClip();
        }
        callback(clip);
    }

    public static string SecondsToTime(float allTime)
    {
        int mins = Mathf.FloorToInt(allTime / 60f);
        int seconds = Mathf.FloorToInt(allTime - mins * 60);

        return mins + ":" + (seconds < 10 ? "0" + seconds : "" + seconds);
    }
    public static int[] SecondsToInts(float all)
    {
        TimeSpan t = TimeSpan.FromSeconds(all);

        return new int[3] { t.Minutes, t.Seconds, t.Milliseconds };
    }

    public static float IntsToSeconds(int[] ints)
    {
        TimeSpan t = new TimeSpan(0, 0, ints[0], ints[1], ints[2]);
        return (float)t.TotalSeconds;
    }
}