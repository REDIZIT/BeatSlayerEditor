using Assets.SimpleLocalization;
using GameNet;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class EditorUpdater : MonoBehaviour
{
    public GameObject updateWindow;
    public Text infoText;

    

    [Header("Install")]
    public GameObject installWindow;
    public Slider progressBar;
    public Text stateText, percentsText;

    string apibase => NetCore.Url_Server;
    public string url_getVersion => apibase + "/Builds/GetEditorVersion";
    public string url_downloadApk => apibase + "/Builds/DownloadEditorApk";


    public void Check()
    {
        WebClient c = new WebClient();
        c.DownloadStringCompleted += OnVersionGot;

        c.DownloadStringAsync(new System.Uri(url_getVersion));
    }

    public void OnInstallClicked()
    {
        WebClient c = new WebClient();
        c.DownloadFileCompleted += OnDownloadComplete;
        c.DownloadProgressChanged += OnDownloadProgress;

        installWindow.SetActive(true);
        progressBar.value = 0;
        stateText.text = "Waiting";
        percentsText.text = "0%";

        c.DownloadFileAsync(new System.Uri(url_downloadApk), Application.persistentDataPath + "/data/bseditor.apk");
    }

    private void OnDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        progressBar.value = e.ProgressPercentage;
        stateText.text = "Downloading..";
        percentsText.text = e.ProgressPercentage + "%";
    }

    private void OnDownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        stateText.text = "Installing..";
        InstallApk();
        progressBar.value = 100;
        percentsText.text = "100%";
    }

    void InstallApk()
    {
        string apkPath = Application.persistentDataPath + "/data/bseditor.apk";

        try
        {
            //Get Activity then Context
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            //Get the package Name
            string packageName = unityContext.Call<string>("getPackageName");
            string authority = packageName + ".fileprovider";

            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);


            int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");

            //File fileObj = new File(String pathname);
            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            //FileProvider object that will be used to call it static function
            AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
            //getUriForFile(Context context, String authority, File file)
            AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
            currentActivity.Call("startActivity", intent);

            stateText.text = "Installed successfully";
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            stateText.text = "Install error: " + e.Message;
        }
    }





    private void OnVersionGot(object sender, DownloadStringCompletedEventArgs e)
    {
        string response = e.Result;
        bool hasUpdate = IsVersionNewer(response);

        if (hasUpdate)
        {
            updateWindow.SetActive(true);
            infoText.text = string.Format(LocalizationManager.Localize("UpdateText"), response, Application.version);
        }
    }

    public bool IsVersionNewer(string version)
    {
        string curVersion = Application.version;

        string[] curNums = curVersion.Split('.');
        string[] verNums = version.Split('.');

        if (curNums.Length == verNums.Length)
        {
            for (int i = 0; i < curNums.Length; i++)
            {
                int curNumber = int.Parse(curNums[i]);
                int verNumber = int.Parse(verNums[i]);

                if (verNumber > curNumber)
                {
                    return true;
                }
                else if (verNumber < curNumber)
                {
                    return false;
                }
            }
            return false;
        }
        else if (curNums.Length > verNums.Length)
        {
            return false;
        }
        else return true;
    }
}