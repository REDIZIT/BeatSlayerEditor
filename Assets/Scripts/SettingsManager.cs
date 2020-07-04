using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class SettingsManager
{
    static Settings _settings;
    public static Settings settings
    {
        get
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings;
        }
        set { _settings = value; }
    }

    public static void Save()
    {
        XmlSerializer formatter = new XmlSerializer(typeof(Settings));
        using (FileStream fs = new FileStream(Application.persistentDataPath + "/settings.xml", FileMode.Create))
        {
            formatter.Serialize(fs, _settings);
        }
    }
    public static void Load()
    {
        if (!File.Exists(Application.persistentDataPath + "/settings.xml")) _settings = new Settings();
        else
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Settings));
            using (FileStream fs = new FileStream(Application.persistentDataPath + "/settings.xml", FileMode.Open))
            {
                _settings = (Settings)formatter.Deserialize(fs);
            }
        }
    }
}

[Serializable]
public class Settings
{
    public bool usePostProcess = true;
    public bool useConsole = false;
    public string nickname = "";
    public string email = "";
    public string lang;
}