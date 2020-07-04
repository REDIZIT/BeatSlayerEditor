using GameNet.Operations;
using ModernEditor.Popup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModernEditor.Importing
{
    public class ProjectImporterUI : MonoBehaviour
    {
        public ProjectUI projectUI;

        [Header("UI")]
        public GameObject downloadOverlay;
        public Slider progressBar;
        public Text progressText;

        public async void OnDownloadProjectBtnClick(PublishedProjectItem item)
        {
            string trackname = item.mapInfo.group.author + "-" + item.mapInfo.group.name;

            downloadOverlay.SetActive(true);
            progressText.text = "Waiting";
            progressBar.value = 0;


            await ProjectImporter.ImportProject(trackname, item.mapInfo.nick, OnDownloadProgress, OnDownloadComplete);
        }

        public void OnDownloadProgress(int progress)
        {
            progressBar.value = progress;
            progressText.text = $"Downloading ({progress}%)";
        }
        public void OnDownloadComplete(OperationMessage msg)
        {
            downloadOverlay.SetActive(false);
            projectUI.ShowProjects();

            if (msg.Type == OperationType.Success)
            {
                PopupMessager.ShowSuccess("Project imported!", "Now you can edit it. You can find it in 'Projects' section");
            }
            else
            {
                PopupMessager.ShowError("Import error!", msg.Message);
            }
        }
    }
}