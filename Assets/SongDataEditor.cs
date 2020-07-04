using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongDataEditor : MonoBehaviour
{
    public Transform audioFilesExplorer;
    public GameObject audioFilesExplorerPrefab;

    public InputField authorField, nameField;


    private void Start()
    {
        RefreshAudioFilesExplorer();
    }


    public void RefreshAudioFilesExplorer()
    {
        foreach(Transform child in audioFilesExplorer)
        {
            Destroy(child.gameObject);
        }

        string[] allFiles = Directory.GetFiles(Application.persistentDataPath + "/AudioFiles");

        for (int i = 0; i < allFiles.Length; i++)
        {
            GameObject item = Instantiate(audioFilesExplorerPrefab, audioFilesExplorer);
            item.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -15 - i * 30);
            item.GetComponentInChildren<Text>().text = Path.GetFileName(allFiles[i]);
            item.name = Path.GetFileNameWithoutExtension(allFiles[i]);
            if (!allFiles[i].Contains(".ogg"))
            {
                item.GetComponent<Button>().enabled = false;
                item.GetComponent<Image>().color = new Color(0.8f, 0, 0, 0.9f);
            }
        }
    }

    public void AudioFileItem_Clicked(GameObject sender)
    {
        authorField.text = sender.name.Split('-')[0];
        nameField.text = sender.name.Split('-')[1];
    }


    public void CreateProject()
    {
        Project project = new Project();
        project.author = authorField.text;
        project.name = nameField.text;

        project.audioFile = File.ReadAllBytes(Application.persistentDataPath + "/AudioFiles/" + project.author + "-" + project.name + ".ogg");


        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create(Application.persistentDataPath + "/Projects/" + project.author + "-" + project.name + ".bsp"))
        {
            binaryFormatter.Serialize(fileStream, project);
        }

        SceneManager.LoadScene(0);
    }

    public void BackToProjectManager()
    {
        SceneManager.LoadScene(0);
    }
}