using Assets.SimpleLocalization;
using InEditor.Legacy;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Testing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class EditorScript : MonoBehaviour
{
    [Header("Основное")]
    public AudioSource aSource;
    public Project project;
    public WarningSystem warningSystem;

    [Header("Инструменты")]
    public InspectorTool inspectorTool;
    public GameObject bookmarkPrefab;

    [Header("Интерфейс")]
    public Image coverImage;
    public Text nameText, authorText, stateText;
    public Slider miniTimeLineSlider;
    public GameObject exitWithoutSaving;

    [Header("Timeline")]
    public Slider timelineSlider;

    [Header("Кубы")]
    public Transform cubeGroup;
    public MeshRenderer[] roads;

    [Header("Таймлайн")]
    public InputField speedField;
    public Image pauseBtnImg;
    public Sprite pauseSprite, playSprite;

    [Header("Управление")]
    public float moveSpeed;
    public float rotationSpeed, zoomingSpeed, cubeRewindSpeed;

    [Header("Визуал")]
    public Transform spectrum;
    public UnityEngine.Gradient spectrumGradient;

    // Hidden
    [HideInInspector] public List<EditorBookmark> bookmarks = new List<EditorBookmark>();

    // Приватные переменные
    float musicMoveSpeed = 0.5f;
    TimeSpan lastSaveTime;
    List<BeatCubeGO> history = new List<BeatCubeGO>();

    private void Awake()
    {
        if (LCData.project == null && enabled) { SceneManager.LoadScene("Menu"); return; }

        HandleSettings();

        InitProject();
        InitBookmarks();

        CreateTimeStepper();
        InitSpectrum();
    }

    private void Update()
    {
        MoveCam();

        HotKeysHandler();
        UpdateMiniTimeSlider();
        UpdateSpectrum();

        cubeGroup.transform.position = new Vector3(0, 0, -aSource.time * 60);

        UpdateTimeline();

        CopyOperationUpdate();

        foreach (MeshRenderer road in roads)
        {
            Color clr = road.material.GetColor("_EmissionColor");
            if (clr != Color.black)
            {
                road.material.SetColor("_EmissionColor", clr * 0.9f);
            }
        }
    }



    // ============ Говно код ============ //
    // [DANGER] [DANGER] [DANGER] [DANGER] //
    // [DANGER] [DANGER] [DANGER] [DANGER] //
    // =================================== //

    #region Управление проектом

    public void SaveProject()
    {
        StartCoroutine(SaveProjectAsync());
    }
    IEnumerator SaveProjectAsync()
    {
        SetState("Сохранение");
        //TheGreat.SaveProject(project);
        ProjectManager.SaveProject(project);
        yield return new WaitForEndOfFrame();
        SetState("");
        lastSaveTime = DateTime.Now.TimeOfDay;
    }

    public void Exit()
    {
        if ((DateTime.Now.TimeOfDay - lastSaveTime).TotalSeconds > 5)
        {
            exitWithoutSaving.SetActive(true);
        }
        else
        {
            ForceExit();
        }
    }
    public void ForceExit()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Undo()
    {
        if (history.Count == 0) return;
        Debug.Log("Undo");

        project.beatCubeList.Remove(history[history.Count - 1].beatCubeClass);
        Destroy(history[history.Count - 1].gameObject);
        history.RemoveAt(history.Count - 1);
    }

    #endregion

    #region Инициализация

    void InitProject()
    {
        project = LCData.project;
        project.CheckDefaults();

        SetState("Загрузка");
        nameText.text = project.name;
        authorText.text = project.author;
        if (project.hasImage) coverImage.sprite = TheGreat.LoadSprite(Application.persistentDataPath + "/Maps/" + project.author + "-" + project.name + "/" + project.author + "-" + project.name + Project.ToString(project.imageExtension));

        LoadBeatCubes();
        StartCoroutine(InitProjectAsync());
    }
    IEnumerator InitProjectAsync()
    {
        yield return InitAudioSource();
        SetState("");

        aSource.Play();
    }

    IEnumerator InitAudioSource()
    {
        //string path = Application.persistentDataPath + "/Projects/tempaudio." + (project.audioExtension == Project.AudioExtension.Mp3 ? "mp3" : "ogg");
        string path = Application.persistentDataPath + "/Maps/" + project.author + "-" + project.name + "/" + project.author + "-" + project.name + Project.ToString(project.audioExtension);
        //File.WriteAllBytes(path, project.audioFile);
        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            aSource.clip = www.GetAudioClip();
        }
        //File.Delete(path);
        timelineSlider.maxValue = aSource.clip.length;
    }

    void CreateTimeStepper()
    {
        Transform timeStepper = GameObject.Find("TimeStepper").transform;
        for (int i = 0; i <= 20; i++)
        {
            GameObject step = Instantiate(timeStepper.GetChild(0).gameObject, timeStepper);
            float height = 1;
            if (i % 5 == 0) height = 3;
            step.transform.localScale = new Vector3(0.2f, height, 0.2f);
            step.transform.localPosition = new Vector3(0, height / 2f, i * 3);
        }
        timeStepper.GetChild(0).gameObject.SetActive(false);
    }

    void HandleSettings()
    {
        SettingsManager.Load();
        bool usePost = SettingsManager.settings.usePostProcess;
        GetComponent<PostProcessVolume>().enabled = usePost;
        GetComponent<PostProcessLayer>().enabled = usePost;
    }


    #endregion

    #region Timeline

    void UpdateTimeline()
    {
        timelineSlider.value = aSource.time;
        timelineSlider.GetComponentInChildren<Text>().text = TheGreat.SecondsToTime(aSource.time) + " / " + TheGreat.SecondsToTime(timelineSlider.maxValue);
    }
    public void OnTimelineSliderChanged()
    {
        aSource.time = timelineSlider.value;
    }

    public void OnSpeedFieldChange()
    {
        if (float.TryParse(speedField.text, out float speed))
        {
            speedField.GetComponentsInChildren<Text>()[1].color = Color.white;
            aSource.pitch = speed;
        }
        else
        {
            speedField.GetComponentsInChildren<Text>()[1].color = Color.red * 0.5f;
        }
    }
    public void OnMoveStepFieldChange(InputField field)
    {
        if (float.TryParse(field.text, out float speed))
        {
            field.GetComponentsInChildren<Text>()[1].color = Color.white;
            musicMoveSpeed = speed;
        }
        else
        {
            field.GetComponentsInChildren<Text>()[1].color = Color.red * 0.5f;
        }
    }

    public void PauseMusic()
    {
        if (aSource.isPlaying)
        {
            aSource.Pause();
            pauseBtnImg.sprite = playSprite;
        }
        else
        {
            aSource.Play();
            pauseBtnImg.sprite = pauseSprite;
        }
    }

    public void MoveMusicTime(int dir)
    {
        float newTime = aSource.time + dir * musicMoveSpeed;
        aSource.time = Mathf.Clamp(newTime, 0, aSource.clip.length);
    }

    #endregion

    void HotKeysHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space)) PauseMusic();
        if (Input.GetKey(KeyCode.A)) MoveMusicTime(-1);
        if (Input.GetKey(KeyCode.D)) MoveMusicTime(1);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) SaveProject();
        if (Input.GetKeyDown(KeyCode.F) && inspectorTool.selectedCube != null) aSource.time = inspectorTool.selectedCube.beatCubeClass.time;
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z)) Undo();
        if (Input.GetKeyDown(KeyCode.Delete)) DestroyCube();

        inspectorTool.HotKeysHandler();
    }

    #region Управление камерой

    bool isCamRotating;
    void MoveCam()
    {
        if (inspectorTool.swipeEditorLocker.activeSelf) return;
        if (EventSystem.current.IsPointerOverGameObject() && !isCamRotating) return;

        if (Application.isEditor)
        {
            isCamRotating = Input.GetMouseButton(0);
        }
        else
        {
            isCamRotating = Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject();
        }
        if (isCamRotating)
        {
            if (Application.isEditor)
            {
                transform.RotateAround(Vector3.zero, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed);
                transform.RotateAround(Vector3.zero, transform.right, -Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed);
            }
            else
            {
                transform.RotateAround(Vector3.zero, Vector3.up, Input.touches[0].deltaPosition.x * Time.deltaTime * rotationSpeed / 400f);
                transform.RotateAround(Vector3.zero, transform.right, -Input.touches[0].deltaPosition.y * Time.deltaTime * rotationSpeed / 400f);
            }
        }

        transform.position += transform.forward * Input.mouseScrollDelta.y * zoomingSpeed;
    }

    #endregion

    #region Бит кубы

    void LoadBeatCubes()
    {
        for (int i = 0; i < project.beatCubeList.Count; i++)
        {
            BeatCubeClass cls = project.beatCubeList[i];
            GameObject cube = Instantiate(cls.type == BeatCubeClass.Type.Line ? linePrefab : cubePrefab, cubesGroup);
            BeatCubeGO script = cube.GetComponent<BeatCubeGO>();
            history.Add(script);
            script.beatCubeClass = cls;
            script.Render();
            cube.transform.localScale = new Vector3(2f, 2f, 2f);
        }
    }

    public GameObject cubePrefab, linePrefab;
    public Transform cubesGroup;
    public void SpawnCube(BeatCubeClass cls)
    {
        GameObject cube = Instantiate(cls.type == BeatCubeClass.Type.Line ? linePrefab : cubePrefab, cubesGroup);
        BeatCubeGO script = cube.GetComponent<BeatCubeGO>();
        history.Add(script);
        script.beatCubeClass = cls;
        project.beatCubeList.Add(cls);

        float roadPos = cls.road * 2.5f - 1.25f;
        float heightPos = cls.level * 2.5f + 1.5f;
        cube.transform.localScale = new Vector3(2f, 2f, 2f);

        script.Render();

        warningSystem.Refresh();
    }
    public void DestroyCube()
    {
        if (inspectorTool.selectedCube == null) return;

        history.Remove(inspectorTool.selectedCube);
        project.beatCubeList.Remove(inspectorTool.selectedCube.beatCubeClass);
        Destroy(inspectorTool.selectedCube.gameObject);

        warningSystem.Refresh();
    }

    #endregion

    #region Закладки

    void InitBookmarks()
    {
        for (int i = 0; i < project.bookmarks.Count; i++)
        {
            Bookmark bk = project.bookmarks[i];
            GameObject go = CreateBookmark(bk.time, bk.color, bk.type);
            bookmarks.Add(new EditorBookmark(bk, go));
        }

        inspectorTool.removeBookmarkBtn.interactable = bookmarks.Count > 0;
    }
    public GameObject CreateBookmark(float time, Color color, int type)
    {
        GameObject go = Instantiate(bookmarkPrefab, cubeGroup);
        go.transform.localPosition = new Vector3(0, 0, time * 60);

        go.transform.GetChild(0).gameObject.SetActive(type == 0 || type == 2);
        go.transform.GetChild(1).gameObject.SetActive(type == 1 || type == 2);

        go.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        go.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
        go.transform.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        go.transform.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);

        return go;
    }
    public void RemoveBookmark(EditorBookmark bookmark)
    {
        Destroy(bookmark.go);
        project.bookmarks.Remove(bookmark.bookmark);
        bookmarks.Remove(bookmark);
    }

    #endregion

    #region Анимации

    public void SetState(string msg)
    {
        if (msg != "")
        {
            stateText.GetComponent<Animator>().Play("StateShow");
            stateText.GetComponentInChildren<UICircle>().ProgressColor = new Color32(0, 128, 255, 255);
            stateText.text = msg;
            stateText.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(stateText.preferredWidth + 20, 0);
        }
        else
        {
            stateText.GetComponent<Animator>().Play("StateHide");
            stateText.GetComponentInChildren<UICircle>().ProgressColor = new Color32(0, 128, 0, 255);
        }
    }
    public void UpdateMiniTimeSlider()
    {
        if (aSource.clip == null) return;
        miniTimeLineSlider.value = aSource.time / aSource.clip.length;

        Text txt = miniTimeLineSlider.transform.GetChild(2).GetComponent<Text>();
        txt.text = TheGreat.SecondsToTime(aSource.time) + " / " + TheGreat.SecondsToTime(aSource.clip.length);
    }
    void InitSpectrum()
    {
        for (int i = 0; i < 256; i++)
        {
            Instantiate(spectrum.GetChild(i).gameObject, spectrum).transform.localPosition = new Vector3(0, 0, i * 0.25f);
        }
        Destroy(spectrum.GetChild(0).gameObject);
    }

    float[] prevsamples = new float[256];
    void UpdateSpectrum()
    {
        float[] samples = new float[256];
        aSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        for (int i = 0; i < samples.Length; i++)
        {
            float smoothSample = (samples[i] + prevsamples[i]) / 2f;
            prevsamples[i] = samples[i];

            float power = smoothSample * 35f * (i / 25f + 1);


            Color clr = spectrumGradient.Evaluate(power) * Mathf.Clamp(power, 0.01f, 3);

            //Color clr = Color.white * Mathf.Clamp(power, 0.01f, 3);
            spectrum.GetChild(i).localScale = new Vector3(0.25f, power, 0.25f);
            spectrum.GetChild(i).localPosition = new Vector3(spectrum.GetChild(i).localPosition.x, power / 2f, spectrum.GetChild(i).localPosition.z);
            spectrum.GetChild(i).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", clr);
        }
    }

    #endregion

    #region Инструменты быстрого доступа
    [Header("Быстрые инструменты")]
    public Button alignBtn;
    public Button magnetBtn, magnetApplyBtn, rewindCubeForward, rewindCubeBackward;

    public void OnBeatCubeSelected()
    {
        alignBtn.interactable = true;
        magnetBtn.interactable = true;
    }
    public void OnBeatCubeDeselected()
    {
        alignBtn.interactable = false;
        magnetBtn.interactable = false;
    }
    public void AlignTo()
    {
        if (inspectorTool.selectedCube == null) return;
        aSource.time = inspectorTool.selectedCube.beatCubeClass.time;
    }
    public void RewindCube(int dir)
    {
        float speed = cubeRewindSpeed * dir;
        inspectorTool.selectedCube.beatCubeClass.time += speed;
        inspectorTool.selectedCube.Render();

        inspectorTool.OpenInspectorTool(); // Refresh inspector
    }

    bool isMagnitig;
    public void MagnetStart()
    {
        if (isMagnitig)
        {
            magnetApplyBtn.gameObject.SetActive(false);
            inspectorTool.isMultiSelecting = false;
            foreach(BeatCubeGO cube in inspectorTool.selectedCubes)
            {
                cube.OnDeselect();
            }
            inspectorTool.selectedCubes.Clear();
            isMagnitig = false;
        }
        else
        {
            isMagnitig = true;
            magnetApplyBtn.gameObject.SetActive(true);
            inspectorTool.isMultiSelecting = true;
        }
    }
    public void MagnetApply()
    {
        foreach(BeatCubeGO cube in inspectorTool.selectedCubes)
        {
            cube.beatCubeClass.time = inspectorTool.selectedCube.beatCubeClass.time;
            cube.Render();
        }
        MagnetStart();
    }


    #region Copying

    public List<BeatCubeGO> copyingCubes = new List<BeatCubeGO>();
    public List<BeatCubeGO> copiedCubes = new List<BeatCubeGO>();
    [HideInInspector] public bool isCopying;
    [HideInInspector] public bool isPlacing;
    float copiedTime;
    public void CopyStart()
    {
        isCopying = true;
    }
    public void CopyApply()
    {
        for (int i = 0; i < copyingCubes.Count; i++)
        {
            GameObject cube = Instantiate(copyingCubes[i].gameObject, copyingCubes[i].transform.parent);
            copyingCubes[i].OnDeselect();
            cube.GetComponent<BeatCubeGO>().OnSelect(Color.cyan);
            copiedCubes.Add(cube.GetComponent<BeatCubeGO>());
        }
        isPlacing = true;
        copiedTime = aSource.time;
    }
    public void CopyPlace()
    {
        isPlacing = false;
        isCopying = false;

        for (int i = 0; i < copiedCubes.Count; i++)
        {
            copiedCubes[i].OnDeselect();
        }

        copiedCubes.Clear();
        copyingCubes.Clear();
    }
    public void CopyOperationUpdate()
    {
        if (!isCopying) return;

        if (!isPlacing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit))
                {
                    BeatCubeGO go = hit.transform.GetComponent<BeatCubeGO>();
                    if (hit.transform.name == "LineCollider") go = hit.transform.parent.GetComponent<BeatCubeGO>();
                    if (go != null)
                    {
                        if (copyingCubes.Contains(go))
                        {
                            go.OnDeselect();
                            copyingCubes.Remove(go);
                        }
                        else
                        {
                            go.OnSelect(Color.cyan);
                            copyingCubes.Add(go);
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < copiedCubes.Count; i++)
            {
                float cubeTime = copyingCubes[i].beatCubeClass.time + (aSource.time - copiedTime);
                copiedCubes[i].beatCubeClass.time = cubeTime;
                copiedCubes[i].Render();
            }
        }
        
    }

    #endregion

    #endregion

    #region Тест

    public void OnTestBtnClick()
    {
        //TestManager.TestOwnMap(project.author + "-" + project.name);
        // Deprecated 
    }

    #endregion
}


public class EditorBookmark
{
    public Bookmark bookmark;
    public GameObject go;

    public EditorBookmark(Bookmark bookmark, GameObject go)
    {
        this.bookmark = bookmark;
        this.go = go;
    }
}