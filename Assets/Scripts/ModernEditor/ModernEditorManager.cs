using System;
using InGame.Game.Spawn;
using Newtonsoft.Json;
using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using ModernEditor.Difficulties;
using Testing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModernEditorManager : MonoBehaviour
{
    public Camera Cam
    {
        get { return GetComponent<Camera>(); }
    }
    public EditorBeatManager bm;
    public DifficultyManager dm;

    public DifficultyUI dmui;
    
    
    public AudioSource asource;

    public Project project;

    [Header("Intruments")] 
    public ModernEditor.Instruments.InspectorTool inspector;
    public Timeline timeline;

    
    [Header("IntrumentsUI")] public GameObject swipeEditorLocker;

    [Header("Quick Instruments")] 
    public MagnetInstrument magnet;
    public CopyInstrument copier;

    
    [Header("UI")] [Space(20)] 
    public Transform overlays;
    public Text tracknameText;
    public Text difficultyText;
    public Image coverImage;
    
    

    private void Start()
    {
        if (LCData.project == null)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
        

        project = LCData.project;
        LCData.project = null;

        if(project.difficulties == null || project.difficulties.Count == 0)
        {
            project = ProjectUpgrader.UpgradeToDifficulty(project);
        }

        if (project.lastGivenDifficultyId == -1)
        {
            project = ProjectUpgrader.UpgradeDifficulties(project);
        }

        bm.CalculateField();
        
        dm.LoadDifficulty(project.difficulties[0]);

        asource.clip = LCData.audioClip;
        
        Sprite coverSprite = ProjectManager.LoadCover(project);
        if (coverSprite != null) coverImage.sprite = coverSprite;


        #if UNITY_EDITOR
        Helpers.UrlsChecker.IsGameWorkingWithLocalhost();
        #endif
    }

    private void Update()
    {
        HandleScreenTouches();

        if (!IsOverlayEnabled())
        {
            HotKeysHandler();           
        }
    }

    private void HotKeysHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space)) timeline.OnPlayPauseBtnClick();

        //if (Input.GetKeyDown(KeyCode.S)) Time.timeScale = 0.2f;
        //else if (Input.GetKeyDown(KeyCode.W)) Time.timeScale = 1f;

        if (Input.GetKey(KeyCode.A)) timeline.Seek(-0.02f, true);
        if (Input.GetKey(KeyCode.D)) timeline.Seek(0.02f, true);

        if (Input.GetKeyDown(KeyCode.N)) inspector.SpawnBeat();
    }

    void HandleScreenTouches()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                IEditorBeat beat = hit.transform.GetComponent<IEditorBeat>();
                if (beat == null) beat = hit.transform.parent.GetComponent<IEditorBeat>();
                
                if (inspector.selectedCube == null)
                {
                    AddCubeToSelected(beat);
                }
                else
                {
                    if (inspector.selectedCube == beat)
                    {
                        RemoveCubeFromSelected(beat);
                        inspector.OnClose();
                    }
                    else if(beat != null)
                    {
                        AddCubeToSelected(beat);
                    }
                }
            }
        }
    }

    void AddCubeToSelected(IEditorBeat beat)
    {
        if (magnet.isSelecting)
        {
            magnet.OnCubePoint(beat);
        }
        else if (copier.isSelecting)
        {
            copier.OnCubePoint(beat);
        }
        else
        {
            inspector.selectedCube?.OnDeselect();
            inspector.selectedCube = beat;
        }
        
        beat.OnPoint();
    }

    void RemoveCubeFromSelected(IEditorBeat beat)
    {
        if (magnet.isSelecting)
        {
            magnet.OnCubeDeselect(beat);
        }
        else if (copier.isSelecting)
        {
            copier.OnCubeDeselect(beat);
        }
        else
        {
            inspector.selectedCube = null;
        }
        
        beat.OnDeselect();
    }

    public void UpdateProjectTexts()
    {
        // ◉○◌◎◯●◌
        tracknameText.text = project.author + "-" + project.name;
        difficultyText.text = "[" + dm.selectedDifficulty.name + " " + 
                              new string('●', dm.selectedDifficulty.stars) +
                              new string('○', 10 - dm.selectedDifficulty.stars) + "]";
    }


    #region Project
    public void TestProject()
    {
        TestManager.TestOwnMap(project.author + "-" + project.name, dm.selectedDifficulty.id);
    }
    
    public void SaveProject()
    {
        ProjectManager.SaveProject(project);
    }
    
    public void Exit()
    {
        SceneManager.LoadScene("Menu");
    }
    
    #endregion

    #region Bool checks

    private bool IsOverlayEnabled()
    {
        foreach (Transform child in overlays)
        {
            foreach (Transform window in child)
                if (window.gameObject.activeSelf) return true;
        }
        return false;
    }
    
    public bool IsPointerOverUIObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
 
        for (int touchIndex = 0; touchIndex < Input.touchCount; touchIndex++)
        {
            Touch touch = Input.GetTouch(touchIndex);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return true;
        }
 
        return false;
    }

    #endregion
}


public static class ProjectUpgrader
{
    /// <summary>
    /// Update project into project with difficulties system (date by 25.04.2020)
    /// </summary>
    public static Project UpgradeToDifficulty(Project legacyProject)
    {
        Debug.Log("UpgradeToDifficulty " + legacyProject.author + "-" + legacyProject.name);
        Project proj = legacyProject;

        proj.difficulties = new List<Difficulty>();
        proj.difficulties.Add(new Difficulty()
        {
            name = "Standard",
            stars = 4,
            beatCubeList = legacyProject.beatCubeList
        });
        proj.beatCubeList = null;

        foreach (var cls in proj.difficulties[0].beatCubeList)
        {
            cls.speed = 1;
            if (cls.type == BeatCubeClass.Type.Line)
            {
                if (cls.linePoints.Count > 0)
                {
                    cls.lineEndRoad = cls.road;
                    cls.lineLenght = cls.linePoints[1].z;
                    cls.lineEndLevel = cls.level;
                    cls.linePoints.Clear();
                }
            }
        }

        
        return proj;
    }

    /// <summary>
    /// Add id in every Difficulty
    /// </summary>
    public static Project UpgradeDifficulties(Project legacyProject)
    {
        Debug.Log("UpgradeDifficulties (give id) " + legacyProject.author + "-" + legacyProject.name);
        Project proj = legacyProject;

        int id = -1;
        for (int i = 0; i < proj.difficulties.Count; i++)
        {
            id++;
            proj.difficulties[i].id = id;
        }

        proj.lastGivenDifficultyId = id;

        return proj;
    }
}