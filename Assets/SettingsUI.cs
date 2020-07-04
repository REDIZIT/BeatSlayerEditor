using Assets.SimpleLocalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Switch consoleSwitch;
    public Dropdown langDropdown;
    public Switch postProcSwitch;
    public Text projectPathText;
    public Text legacyProjectPathText;

    [Header("Connects")]
    public GameObject console;

    public void Init()
    {
        SettingsManager.Load();

        consoleSwitch.Set(SettingsManager.settings.useConsole);
        langDropdown.value = SettingsManager.settings.lang == "English" ? 0 : 1;
        postProcSwitch.Set(SettingsManager.settings.usePostProcess);

        consoleSwitch.onChangeCallback = OnChange;
        postProcSwitch.onChangeCallback = OnChange;

        string projectFolderPath = Application.persistentDataPath + "/Maps";
        string legacyFolderPath = Application.persistentDataPath + "/Projects";
        projectPathText.text = LocalizationManager.Localize("ProjectFolder") + $": <color=#777>{projectFolderPath}</color>";
        legacyProjectPathText.text = LocalizationManager.Localize("LegacyProjectFolder") + $": <color=#222>{legacyFolderPath}</color>";

        HandleSettings();
    }

    public void HandleSettings()
    {
        console.SetActive(SettingsManager.settings.useConsole);

        LocalizationManager.Language = SettingsManager.settings.lang;
    }



    public void OnChange(Dropdown d)
    {
        if (d.name == langDropdown.name) SettingsManager.settings.lang = d.value == 0 ? "English" : "Russian";

        SettingsManager.Save();

        HandleSettings();
    }
    public void OnChange(Switch s)
    {
        if (s.name == consoleSwitch.name) SettingsManager.settings.useConsole = s.isOn;
        else if (s.name == postProcSwitch.name) SettingsManager.settings.usePostProcess = s.isOn;

        SettingsManager.Save();

        HandleSettings();
    }
}
