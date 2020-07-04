using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MobileExplorer : MonoBehaviour
{
    public string extension;

    public Text pathText;
    public Sprite folderSprite, fileSprite;

    int selectedDrive;
    public string path;

    public Transform driversContent, filesContent;

    private void Awake()
    {
        Open(".ico");
    }
    void Init()
    {
        // Init
        path = DriveInfo.GetDrives()[0].RootDirectory.FullName;

        UpdateDrivers();
        UpdateFiles();
    }

    public void Open(string ext)
    {
        extension = ext;
        Init();
        Show();
    }

    public void Close()
    {
        StartCoroutine(ToggleAnimation(0));
    }
    void Show()
    {
        StartCoroutine(ToggleAnimation(1));
    }
    IEnumerator ToggleAnimation(float target)
    {
        Debug.Log("Target " + target);
        CanvasGroup group = GetComponent<CanvasGroup>();
        while(Mathf.Round(group.alpha * 100) / 100f != target)
        {
            Debug.Log("..");
            group.alpha += (target - group.alpha) / 8f;
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Reached");
        group.alpha = target;
        group.blocksRaycasts = target == 1;
    }




    void UpdateDrivers()
    {
        foreach (Transform child in driversContent) if (child.name != "Item") Destroy(child.gameObject);
        driversContent.GetChild(0).gameObject.SetActive(true);

        float height = 0;
        DriveInfo[] drivers = DriveInfo.GetDrives();
        int id = 0;
        for (int i = 0; i < drivers.Length; i++)
        {
            if (!drivers[i].IsReady) continue;
            id++;
            GameObject item = Instantiate(driversContent.GetChild(0).gameObject, driversContent);
            if(i == selectedDrive) item.GetComponent<Image>().color = new Color(0.45f, 0.25f, 0);
            item.transform.GetChild(1).GetComponent<Text>().text = drivers[i].VolumeLabel + "(" + drivers[i].Name + ")";


            string spaceStr;
            long freeSpaceInMegabytes = (drivers[i].TotalSize - drivers[i].AvailableFreeSpace) / 1024L / 1024L;
            if (freeSpaceInMegabytes >= 1024) spaceStr = freeSpaceInMegabytes / 1024 + "Гб / ";
            else spaceStr = freeSpaceInMegabytes + "Мб / ";

            long totalSpaceInMegabytes = (drivers[i].TotalSize) / 1024L / 1024L;
            if (totalSpaceInMegabytes >= 1024) spaceStr += totalSpaceInMegabytes / 1024 + "Гб";
            else spaceStr += totalSpaceInMegabytes + "Мб";

            item.transform.GetChild(2).GetChild(2).GetComponent<Text>().text = spaceStr;
            item.transform.GetChild(2).GetComponent<Slider>().maxValue = totalSpaceInMegabytes;
            item.transform.GetChild(2).GetComponent<Slider>().value = freeSpaceInMegabytes;

            Color clr = new Color(0, 0.5f, 1);
            float freeSpaceInPercents = (float)freeSpaceInMegabytes / (float)totalSpaceInMegabytes;
            if (freeSpaceInPercents >= 80) clr = new Color(1, 0.6f, 0);
            if (freeSpaceInPercents >= 90) clr = new Color(1, 0.1f, 0);

            item.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>().color = clr;

            item.GetComponent<Button>().onClick.AddListener(delegate { OnDriveBtnClicked(id - 1); });
            height += 180;
        }

        driversContent.GetChild(0).gameObject.SetActive(false);
        driversContent.GetComponent<RectTransform>().sizeDelta = new Vector2(driversContent.GetComponent<RectTransform>().sizeDelta.x, height);
    }

    void UpdateFiles()
    {
        foreach (Transform child in filesContent) if (child.name != "Item") Destroy(child.gameObject);
        pathText.text = path;

        if (path == "") return;

        string[] folders = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        float contentHeight = 0;

        filesContent.GetChild(0).gameObject.SetActive(true);
        for (int i = 0; i < folders.Length; i++)
        {
            CreateFolderItem(folders[i]);
            contentHeight += 180;
        }
        for (int i = 0; i < files.Length; i++)
        {
            CreateFileItem(files[i]);
            contentHeight += 180;
        }

        filesContent.GetChild(0).gameObject.SetActive(false);
        filesContent.GetComponent<RectTransform>().sizeDelta = new Vector2(filesContent.GetComponent<RectTransform>().sizeDelta.x, contentHeight);
        filesContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }
    void CreateFileItem(string filepath)
    {
        GameObject item = Instantiate(filesContent.GetChild(0).gameObject, filesContent);
        item.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = fileSprite;
        item.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = Path.GetFileName(filepath);
        item.GetComponent<Button>().interactable = Path.GetExtension(filepath) == extension;
    }
    void CreateFolderItem(string folderpath)
    {
        GameObject item = Instantiate(filesContent.GetChild(0).gameObject, filesContent);
        item.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = folderSprite;
        item.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = Path.GetFileName(folderpath);
        item.GetComponent<Button>().onClick.AddListener(delegate () { OnFolderButtonClicked(folderpath); });
    }




    public void OnFolderButtonClicked(string folderpath)
    {
        path = folderpath;
        UpdateFiles();
    }
    public void OnBackBtnClicked()
    {
        if (path == "") return;
        DirectoryInfo info = new DirectoryInfo(path);
        if(info.Parent == null)
        {
            selectedDrive = -1;
            path = "";
            UpdateDrivers();
        }
        else
        {
            path = info.Parent.FullName;
        }
        UpdateFiles();
    }
    public void OnDriveBtnClicked(int drive)
    {
        selectedDrive = drive;
        path = DriveInfo.GetDrives()[drive].RootDirectory.FullName;
        UpdateDrivers();
        UpdateFiles();
    }
}