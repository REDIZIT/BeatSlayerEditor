using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using ProjectManagement;
using Testing;
using UnityEngine;
using UnityEngine.UI;
using Assets.AccountManagement;
using GameNet;
using TMPro;
using InEditor.Analyze;
using ModernEditor.Popup;
using Michsky.UI.ModernUIPack;

public class ModerationUI : MonoBehaviour
{
    public AccountUI accountUI { get { return GetComponent<AccountUI>(); } }
    public ProjectAnalyzer analyzer;



    public GameObject requestModeraionLocker;
    public GameObject requestModeraionResultLocker;

    public List<ModerateOperation> moderationStates;

    public GameObject requestDetailsOverlay;
    public Text requestDetailsText;

    public Transform moderationListContent;
    public GameObject responseOverlay, noProjectsToCheckText;

    [Header("Approving")]
    public List<Toggle> approvingToggles;
    public Button approveBtn, rejectBtn;
    public TMP_InputField commentField;

    [Header("Analyze")]
    public Text AnalyzeHeaderText;
    public Text MaxScoreText, MaxRPText, CubesCountText, LinesCountText, ScorePerBlockText, RPPerBlockText;
    public HorizontalSelector difficultySelector;
    private List<AnalyzeResult> analyzeResults;


    [Header("DownloadAndTest")]
    public GameObject DAT_overlay;
    public GameObject DAT_closeBtn;
    public Slider DAT_progressBar;
    public Text DAT_stateText, DAT_progressText;


    public string url_moderRequest => NetCore.Url_Server + "/Moderation/CreateRequest?trackname={0}&nick={1}";
    public string url_getOperations => NetCore.Url_Server + "/Moderation/GetOperations";
    public string url_sendResponse => NetCore.Url_Server + "/Moderation/SendResponse?opJson={0}";
    public string url_cheat => NetCore.Url_Server + "/Moderation/ModeratorCheat?nick={0}&trackname={1}";
    public string url_downloadMap => NetCore.Url_Server + "/Moderation/DownloadMap?trackname={0}&nick={1}";

    PublishedProjectItem selectedItem;
    ModerationItem selectedResponseItem;



    private void Start()
    {
        foreach (var toggle in approvingToggles)
        {
            toggle.onValueChanged.AddListener(OnApproveToggleValueChange);
        }
        commentField.onValueChanged.AddListener((str) => OnApproveToggleValueChange(true));
    }




    public void OnRequestModeration(PublishedProjectItem item)
    {
        selectedItem = item;
        requestModeraionLocker.SetActive(true);
    }
    public void OnRequestModerationApply()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) { requestModeraionLocker.SetActive(false); return; }


        string trackname = selectedItem.mapInfo.group.author + "-" + selectedItem.mapInfo.group.name;
        string nick = selectedItem.mapInfo.nick;

        WebClient c = new WebClient();
        string url = string.Format(url_moderRequest, trackname, nick);
        string response = c.DownloadString(url);

        if(response.Contains("[ERR]"))
        {
            Debug.LogError("Request moder error: " + response);
            requestModeraionLocker.SetActive(false);
        }
        else
        {
            requestModeraionResultLocker.gameObject.SetActive(true);
        }
    }

    public void GetOperationStates()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) { moderationStates = new List<ModerateOperation>(); return; }

        WebClient c = new WebClient();
        string url = url_getOperations;
        string response = c.DownloadString(url);

        moderationStates = JsonConvert.DeserializeObject<List<ModerateOperation>>(response);
    }
    public void ShowDetails(PublishedProjectItem item)
    {
        ModerateOperation op = moderationStates.Find(c => c.trackname == item.mapInfo.group.author + "-" + item.mapInfo.group.name && c.nick == AccountManager.LoggedAccount.Nick);
        requestDetailsOverlay.SetActive(true);

        string stateText = op.state == ModerateOperation.State.Waiting ? "<color=#888>Waiting</color>" :
            op.state == ModerateOperation.State.Rejected ? "<color=#F40>Rejected</color>" :
            "<color=#3F3>Approved</color>";

        string moderatorNick = op.state == ModerateOperation.State.Waiting ? "" : $"Moderator nick: {op.moderatorNick}";
        string moderatorComment = op.state == ModerateOperation.State.Waiting ? "" :
            op.state == ModerateOperation.State.Rejected ? $"Comment:\n<color=#F40>{op.moderatorComment}</color>" :
            $"Comment:\n<color=#3F3>{op.moderatorComment}</color>";

        requestDetailsText.text = $"Trackname: {op.trackname}\nNick: {op.nick}\n\n<size=64>{stateText}</size>\n\n{moderatorNick}\n{moderatorComment}";
    }



    public void ShowModerationRequests()
    {
        foreach (Transform child in moderationListContent) if (child.name != "ProjectItem") Destroy(child.gameObject);
        GameObject prefab = moderationListContent.GetChild(0).gameObject;
        prefab.SetActive(false);

        Camera.main.GetComponent<ModerationUI>().GetOperationStates();

        prefab.SetActive(true);

        IEnumerable<ModerateOperation> ops = moderationStates.Where(c => c.state == ModerateOperation.State.Waiting);

        moderationListContent.GetComponent<RectTransform>().sizeDelta = new Vector2(moderationListContent.GetComponent<RectTransform>().sizeDelta.x, -8);
        foreach (var request in ops)
        {
            GameObject go = Instantiate(prefab, moderationListContent);
            ModerationItem item = go.GetComponent<ModerationItem>();
            item.operation = request;
            item.Refresh();

            moderationListContent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 260 + 8);
        }

        prefab.SetActive(false);

        noProjectsToCheckText.SetActive(ops.Count() == 0);
    }

    public void OnResponseBtnClicked(ModerationItem item)
    {
        Project proj = ProjectManager.LoadProject(item.operation);
        if (proj == null)
        {
            PopupMessager.ShowError("No downloaded project", "For analyzing project you should download this project");
            return;
        }

        responseOverlay.SetActive(true);
        ResetApproveWindow();

        analyzeResults = analyzer.Analyze(proj);
        AnalyzeResult firstResult = analyzeResults[0];

        difficultySelector.itemList = new List<HorizontalSelector.Item>();
        foreach (var result in analyzeResults)
        {
            var selectorItem = new HorizontalSelector.Item()
            {
                itemTitle = $"#{result.DifficultyId} {result.DifficultyStars}*"
            };

            selectorItem.onValueChanged = new UnityEngine.Events.UnityEvent();
            selectorItem.onValueChanged.AddListener(OnDifficultySelectionValueChange);

            difficultySelector.itemList.Add(selectorItem);
        }

        difficultySelector.RefreshTitle();
        OnDifficultySelectionValueChange();

        selectedResponseItem = item;
    }
    public void OnDifficultySelectionValueChange()
    {
        AnalyzeResult result = analyzeResults[difficultySelector.index];

        AnalyzeHeaderText.text = $"{result.DifficultyName} {result.DifficultyStars}*";
        MaxScoreText.text = Mathf.FloorToInt(result.MaxScore * 10) / 10f + "";
        MaxRPText.text = Mathf.FloorToInt(result.MaxRP * 10) / 10f + "";
        CubesCountText.text = result.CubesCount + "";
        LinesCountText.text = result.LinesCount + "";
        ScorePerBlockText.text = Mathf.FloorToInt(result.ScorePerBlock * 10) / 10f + "";
        RPPerBlockText.text = Mathf.FloorToInt(result.RPPerBlock * 10) / 10f + "";
    }
    private void ResetApproveWindow()
    {
        commentField.text = "";
        foreach (var toggle in approvingToggles)
        {
            toggle.isOn = false;
        }
        approvingToggles[approvingToggles.Count - 1].isOn = true;
    }



    public void OnResponseVerify()
    {
        selectedResponseItem.operation.state = ModerateOperation.State.Approved;
        selectedResponseItem.operation.moderatorNick = AccountManager.LoggedAccount.Nick;
        selectedResponseItem.operation.moderatorComment = commentField.text;
        SendResponse();

        responseOverlay.SetActive(false);
    }
    public void OnResponseReject()
    {
        selectedResponseItem.operation.state = ModerateOperation.State.Rejected;
        selectedResponseItem.operation.moderatorNick = AccountManager.LoggedAccount.Nick;
        selectedResponseItem.operation.moderatorComment = commentField.text;
        SendResponse();

        responseOverlay.SetActive(false);
    }
    void SendResponse()
    {
        WebClient c = new WebClient();
        string url = string.Format(url_sendResponse, JsonConvert.SerializeObject(selectedResponseItem.operation));
        string response = c.DownloadString(url);

        if(selectedResponseItem.operation.nick == selectedResponseItem.operation.moderatorNick)
        {
            c.DownloadString(string.Format(url_cheat, selectedResponseItem.operation.moderatorNick, selectedResponseItem.operation.trackname));
        }

        ShowModerationRequests();
    }


    public void OnApproveToggleValueChange(bool b)
    {
        bool canApprove = approvingToggles.All(c => c.isOn);
        bool interactable = !string.IsNullOrWhiteSpace(commentField.text);

        approveBtn.gameObject.SetActive(canApprove);
        rejectBtn.gameObject.SetActive(!canApprove);

        approveBtn.interactable = interactable;
        rejectBtn.interactable = interactable;
    }



    #region Download and Test

    ModerateOperation dat_op;
    public void DownloadModerationMap(ModerateOperation op)
    {
        dat_op = op;

        DAT_overlay.SetActive(true);
        DAT_closeBtn.SetActive(false);

        WebClient c = new WebClient();
        c.DownloadFileCompleted += OnModerationMapDownloaded;
        c.DownloadProgressChanged += OnModeraionMapProgress;

        string url = string.Format(url_downloadMap, op.trackname, op.nick);

        // Path where bs will search maps
        string perstPath = Application.persistentDataPath.Replace("com.REDIZIT.BeatSlayerEditor", "com.REDIZIT.BeatSlayer");
        if(Application.isEditor) perstPath = Application.persistentDataPath.Replace("Beat Slayer Editor", "Beat Slayer");

        string mapFolder = perstPath + "/data/moderation";
        if (!Directory.Exists(mapFolder)) Directory.CreateDirectory(mapFolder);

        string filepath = mapFolder + "/" + op.trackname + ".bsz";
        c.DownloadFileAsync(new System.Uri(url), filepath);
    }

    private void OnModeraionMapProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        DAT_progressBar.value = e.ProgressPercentage;
        DAT_progressText.text = e.ProgressPercentage + "%";
        DAT_stateText.text = "Downloading..";
    }

    private void OnModerationMapDownloaded(object sender, AsyncCompletedEventArgs e)
    {
        if (!e.Cancelled && e.Error != null) Debug.LogError("OnModerationMapDownloaded()\n" + e.Error);
        DAT_stateText.text = "Downloaded";
        DAT_closeBtn.SetActive(true);
        
        string perstPath = Application.persistentDataPath.Replace("com.REDIZIT.BeatSlayerEditor", "com.REDIZIT.BeatSlayer");
        if(Application.isEditor) perstPath = Application.persistentDataPath.Replace("Beat Slayer Editor", "Beat Slayer");

        string mapFolder = perstPath + "/data/moderation";
        Directory.CreateDirectory(mapFolder);
        Directory.CreateDirectory(mapFolder + "/map");

        string bszFilePath = mapFolder + "/" + dat_op.trackname + ".bsz";

        ProjectManager.UnpackBszFile(bszFilePath, mapFolder + "/map");

        File.Delete(bszFilePath);
        
        TestManager.TestModerationMap(dat_op.trackname);
    }

    #endregion



    public class ModerateOperation
    {
        public string trackname, nick;

        public enum State
        {
            Waiting, Rejected, Approved
        }
        public State state;

        public enum UploadType
        {
            Requested, Updated
        }
        public UploadType uploadType;


        public string moderatorNick;
        public string moderatorComment;
    }
}