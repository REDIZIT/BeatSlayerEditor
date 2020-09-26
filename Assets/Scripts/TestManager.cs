using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProjectManagement;
using UnityEngine;

namespace Testing
{
    public static class TestManager
    {
        public static string GameFolderPath
        { 
            get
            { 
                if(Application.isEditor) return new DirectoryInfo(Application.persistentDataPath).Parent + "/Beat Slayer";
                return new DirectoryInfo(Application.persistentDataPath).Parent.Parent + "/com.REDIZIT.BeatSlayer/files"; 
            }
        }



        public static void TestOwnMap(string trackname, int difficultyId)
        {
            TestRequest request = new TestRequest(TestType.OwnMap, trackname, difficultyId);
            SaveRequest(request);
            OpenGame();
        }
        public static void TestModerationMap(string trackname)
        {
            TestRequest request = new TestRequest(TestType.ModerationMap, trackname, 0);
            SaveRequest(request);
            OpenGame();
        }



        public static void SaveRequest(TestRequest request)
        {
            if (!Directory.Exists(GameFolderPath + "/data/moderation")) Directory.CreateDirectory(GameFolderPath + "/data/moderation");

            string filepath = GameFolderPath + "/data/moderation/request.json";
            string json = JsonConvert.SerializeObject(request);
            File.WriteAllText(filepath, json);
        }

        public static void OpenGame()
        {
            if (Application.isEditor) { Debug.Log("Open BS"); return; }

            bool fail = false;
            string bundleId = "com.REDIZIT.BeatSlayer"; // your target bundle id
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaObject launchIntent = null;
            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
            }
            catch (System.Exception e)
            {
                fail = true;
            }

            if (fail)
            { //open app in store
                Debug.LogError("OpenBS() failed");
                //Application.OpenURL("https://google.com");
            }
            else //open the app
                ca.Call("startActivity", launchIntent);

            up.Dispose();
            ca.Dispose();
            packageManager.Dispose();
            launchIntent.Dispose();
        }
    }


    public class TestRequest
    {
        public TestType type;
        public string trackname;
        public string filepath;
        public int difficultyId;

        public TestRequest(TestType type)
        {
            this.type = type;
        }
        public TestRequest(TestType type, string trackname, int difficultyId)
        {
            this.type = type;
            this.trackname = trackname;
            Debug.Log("Create test request " + trackname);
            this.difficultyId = difficultyId;
        }

        public TestRequest() { }
    }

    public enum TestType
    {
        OwnMap, ModerationMap
    }
}