using GameNet.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace GameNet.WebAPI
{
    public static class WebAPI
    {
        static string apibase => NetCore.Url_Server;
        public static string url_uploadproject = apibase + "/Maps/Upload";


        static WebAPI()
        {
            AllowHttp();
        }


        public static void UploadProject(byte[] bytes, string trackname, Action<int> progressCallback, Action<OperationResult> resultCallback)
        {
            try
            {
                AllowHttp();

                CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();

                var httpContent = new CI.HttpClient.MultipartFormDataContent();

                CI.HttpClient.ByteArrayContent content = new CI.HttpClient.ByteArrayContent(bytes, "multipart/form-data");
                httpContent.Add(content, "file", trackname + ".bsz");

                Debug.Log("Upload to " + url_uploadproject);

                client.Post(new Uri(url_uploadproject), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
                {
                    string json = r.ReadAsString();
                    Debug.LogWarning("[PUBLISH REPONSE] " + json);
                    OperationResult msg = JsonConvert.DeserializeObject<OperationResult>(json);

                    resultCallback(msg);

                }, (u) => progressCallback?.Invoke(u.PercentageComplete));
            }
            catch(Exception err)
            {
                Debug.LogError(err);
                resultCallback(new OperationResult(OperationResult.State.Fail, "Can't upload due to " + err));
            }
        }


        private static void AllowHttp()
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, e) => { return true; };
        }
    }
}