using Assets.AccountManagement;
using ModernEditor.Importing;
using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ModerationUI;

public class PublishedProjectItem : MonoBehaviour
{
    ModerationUI moderationUI { get { return Camera.main.GetComponent<ModerationUI>(); } }
    public ProjectImporterUI importerUI;

    public MapInfo mapInfo;

    public RawImage coverImage;
    public Text authorText, nameText;

    public Text likesText, dislikesText, playCountText, downloadsText;

    public Text publishTimeText;

    public GameObject requestModerationBtn, showRequestDetailsBtn;
    public Text requestStateText;

    public void Refresh()
    {
        if (mapInfo == null) return;

        authorText.text = mapInfo.group.author;
        nameText.text = mapInfo.group.name;

        likesText.text = mapInfo.likes + "";
        dislikesText.text = mapInfo.dislikes + "";
        playCountText.text = mapInfo.playCount + "";
        downloadsText.text = mapInfo.downloads + "";

        publishTimeText.text = mapInfo.publishTime.ToString("MM.dd.yyyy HH:mm:ss");


        ModerateOperation moderateOperation = moderationUI.moderationStates.Find(c => c.trackname == mapInfo.group.author + "-" + mapInfo.group.name && c.nick == AccountManager.LoggedAccount.Nick);
        if(moderateOperation == null)
        {
            requestModerationBtn.SetActive(true);
            showRequestDetailsBtn.SetActive(false);
        }
        else
        {
            showRequestDetailsBtn.SetActive(true);
            requestStateText.text = moderateOperation.state.ToString() + (moderateOperation.state == ModerateOperation.State.Waiting ? "" : " by " + moderateOperation.moderatorNick);

            if (moderateOperation.state == ModerateOperation.State.Approved)
            {
                requestModerationBtn.SetActive(false);
                showRequestDetailsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(460, showRequestDetailsBtn.GetComponent<RectTransform>().anchoredPosition.y);
                requestStateText.GetComponent<RectTransform>().anchoredPosition = new Vector2(460 + 419 + 60, requestStateText.GetComponent<RectTransform>().anchoredPosition.y);
            }
        }
    }

    public void OnDownloadBtnClick()
    {
        importerUI.OnDownloadProjectBtnClick(this);
    }
}
