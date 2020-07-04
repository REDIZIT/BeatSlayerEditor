using GameNet;
using GameNet.Operations;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace ModernEditor.Importing
{
    /// <summary>
    /// Allows download and unpack already published maps
    /// </summary>
    public static class ProjectImporter
    {
        static string apibase => NetCore.Url_Server;
        public static string url_downloadProject = apibase + "/Maps/Download?trackname={0}&nick={1}";

        static string tempFolderPath = Application.persistentDataPath + "/temp";
        static string mapsFolderPath = Application.persistentDataPath + "/Maps";

        
        public static async Task ImportProject(string trackname, string nick, Action<int> progess, Action<OperationMessage> complete)
        {
            try
            {
                await DownloadProject(trackname, nick, progess);
            }
            catch(Exception err)
            {
                Debug.LogError(err);
                complete(new OperationMessage(OperationType.Fail, "Download error. See console logs for details"));
                return;
            }

            try
            {
                UnpackProject(trackname);
            }
            catch(Exception err)
            {
                Debug.LogError(err);
                complete(new OperationMessage(OperationType.Fail, "Unpack error. See console logs for details"));
                return;
            }

            complete(new OperationMessage(OperationType.Success));
        }

        static async Task DownloadProject(string trackname, string nick, Action<int> progress)
        {
            WebClient c = new WebClient();
            c.DownloadProgressChanged += (s, e) => { progress(e.ProgressPercentage); };

            string url = string.Format(url_downloadProject, trackname, nick);
            string filename = tempFolderPath + "/" + trackname + ".bsz";

            Directory.CreateDirectory(tempFolderPath);

            await c.DownloadFileTaskAsync(url, filename);
        }

        static void UnpackProject(string trackname)
        {
            string mapFolder = mapsFolderPath + "/" + trackname;
            string tempFilePath = tempFolderPath + "/" + trackname + ".bsz";

            ProjectManager.UnpackBszFile(tempFilePath, mapFolder);

            File.Delete(tempFilePath);
        }
    }
}