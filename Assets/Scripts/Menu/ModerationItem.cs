using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ModerationUI;

public class ModerationItem : MonoBehaviour
{
    public ModerationUI ModerationUI { get { return Camera.main.GetComponent<ModerationUI>(); } }
    public ModerateOperation operation;

    public Text nameText, authorText;
    public Text uploadTypeText;


    public void Refresh()
    {
        authorText.text = operation.trackname.Split('-')[0];
        nameText.text = operation.trackname.Split('-')[1];
        uploadTypeText.text = operation.uploadType.ToString();
    }
    public void OnResponseClicked()
    {
        ModerationUI.OnResponseBtnClicked(this);
    }
    public void OnTestClicked()
    {
        ModerationUI.DownloadModerationMap(operation);
    }
}
