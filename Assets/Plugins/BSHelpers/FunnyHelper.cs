#if UNITY_EDITOR
/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;


namespace BSHelpers.Funny
{
    public static class FunnyHelper
    {
        public static string ScriptsFolderPath
        {
            get
            {
                return Application.dataPath + "/Plugins/BSHelpers";
            }
        }
        
        // Funny settings
        public static bool useBuildCounter = true;
        public static string BuildCounterPath
        {
            get
            {
                return ScriptsFolderPath + "/counter.json";
            }
        }




        public static void OnBuild()
        {
            if (!useBuildCounter) return;

            BuildCounterData data;
            if (File.Exists(BuildCounterPath))
            {
                string json = File.ReadAllText(BuildCounterPath);
                data = JsonConvert.DeserializeObject<BuildCounterData>(json);
            }
            else
            {
                data = new BuildCounterData();
            }

            
            
            data.buildsCount++;

            TimeSpan diffBetweenBuilds = DateTime.Now - data.prevBuildTime;
            if (diffBetweenBuilds.TotalMinutes <= 20)
            {
                data.inEditorTime += diffBetweenBuilds;
            }
            data.prevBuildTime = DateTime.Now;

            File.WriteAllText(BuildCounterPath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }

    
    [InitializeOnLoad]
    class BuildsCounter
    {
        static BuildsCounter()
        {
            if (Application.isPlaying) return;
            FunnyHelper.OnBuild();
        }
    }


    class BuildCounterData
    {
        public int buildsCount;
        public TimeSpan inEditorTime;
        public DateTime prevBuildTime;

        public BuildCounterData()
        {
            
        }
    }
}*/


#endif