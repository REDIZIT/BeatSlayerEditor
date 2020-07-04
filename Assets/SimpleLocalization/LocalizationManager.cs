﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.SimpleLocalization
{
    /// <summary>
    /// Localization manager.
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>
        /// Fired when localization changed.
        /// </summary>
        public static event Action LocalizationChanged = () => { };

        private static readonly Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
        private static string _language = "Unknown";

        /// <summary>
        /// Get or set language.
        /// </summary>
        public static string Language
        {
            get { return _language; }
            set { _language = value; LocalizationChanged(); }
        }

        /// <summary>
        /// Set default language.
        /// </summary>
        public static void AutoLanguage()
        {
            Language = SettingsManager.settings.lang;
        }

        /// <summary>
        /// Read localization spreadsheets.
        /// </summary>
        public static void Read(string path = "Translating")
        {
            if (Language == "Unknown") AutoLanguage();
            Debug.Log("Read with " + Language);

            //if (Dictionary.Count > 0) return;

            var textAssets = Resources.LoadAll<TextAsset>(path);

            foreach (var textAsset in textAssets)
            {
                var text = ReplaceMarkers(textAsset.text);
                var matches = Regex.Matches(text, "\"[\\s\\S]+?\"");


                foreach (Match match in matches)
                {
                    text = text.Replace(match.Value, match.Value.Replace("\"", null).Replace(",", "[comma]").Replace("\n", "[newline]"));
                }


                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var languages = lines[0].Split('|').Select(i => i.Trim()).ToList();


                for (var i = 1; i < languages.Count; i++)
                {
                    if (!dictionary.ContainsKey(languages[i]))
                    {
                        dictionary.Add(languages[i], new Dictionary<string, string>());
                    }
                }

                for (var i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split('|').Select(j => j.Trim()).Select(j => j.Replace("[comma]", ",").Replace("[newline]", "\n").Replace("[N]", "\n")).ToList();
                    var key = columns[0];

                    for (var j = 1; j < languages.Count; j++)
                    {
                        if (key.Contains("//") || key == "" || key == "\n" || key == @"
" || key == " ") continue;

                        if(dictionary[languages[j]].ContainsKey(key))
                        {
                            Debug.LogWarning("Lang: already has key " + key);
                        }
                        else dictionary[languages[j]].Add(key, columns[j]);
                    }
                }
            }

            //AutoLanguage();
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public static string Localize(string localizationKey)
        {
            if (dictionary.Count == 0)
            {
                Read("Localization/Menu");
                Read("Localization/Develop");
            }

            if (!dictionary.ContainsKey(Language)) { Debug.LogError("Language not found: " + Language); return "[ERR L]"; }
            if (!dictionary[Language].ContainsKey(localizationKey)) { Debug.LogError("Translation not found: " + localizationKey); return "[ERR T]"; }

            return dictionary[Language][localizationKey];
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public static string Localize(string localizationKey, params object[] args)
        {
            var pattern = Localize(localizationKey);

            return string.Format(pattern, args);
        }

        private static string ReplaceMarkers(string text)
        {
            return text.Replace("[Newline]", "\n");
        }
    }
}