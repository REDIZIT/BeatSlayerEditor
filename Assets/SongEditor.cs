using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongEditor : MonoBehaviour
{
    SecurityScript ss;
    [Header("Refs")]
    public CustomExplorer explorer;

    [Header("Project")]
    public Text projectNameText;
    public Image projectCoverImage;
    public Transform CamMove;
    public Transform instrumentTab;

    public AudioSource aSource;
    public AudioSource tickSource;

    public Project project;
    string projectFullPath;

    public TextMeshPro timeTMP, miliTimeTMP;

    public Slider timelineSlider;
    public InputField timeStepField;
    public Transform goToTimeFieldsParent;

    public Text ps_pathText, ps_sizeText;
    public Toggle ps_useBeatCubeSoundToggle;

    public Text fpsText;
    public Slider timeSlider;
    public Text timeText;

    public Button[] magnetBtns;
    public Button forceToBtn;

    [Header("SwipeEditor")]
    public GameObject swipeEditorLocker;

    List<GameObject> bcubesHistory = new List<GameObject>();


    public void SettingsChange(Toggle toggle)
    {
        if(toggle.name == "UseBeatCubeSound_Toggle")
        {
            ss.prefs.SetBool("useBeatCubeSound", toggle.isOn);
        }
    }
    public void SettingsChange(InputField field)
    {
        if(field.name == "SpeedField")
        {
            SetSpeed(float.Parse(field.text));
        }
        else if(field.name == "BeatCubesSpeedField")
        {
            ss.prefs.SetKey("BeatCubeSpeed", field.text);
        }
    }
    public void SetSpeed(float speed) { aSource.pitch = speed; }

    private void Awake()
    {
        ss = GetComponent<SecurityScript>();

        if (LCData.loadingProjectName == "")
        {
            SceneManager.LoadScene(0);
            Debug.LogError("LCData.loadingProjectName is null");
            return;
        }
    }
    private void Start()
    {

        projectFullPath = Application.persistentDataPath + "/Projects/" + LCData.loadingProjectName + ".bsp";
        ps_pathText.text = projectFullPath;

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Open(projectFullPath, FileMode.Open))
        {
            project = (Project)binaryFormatter.Deserialize(fileStream);
        }
        FileInfo info = new FileInfo(projectFullPath);
        ps_sizeText.text = "Project size:  " + Mathf.RoundToInt((float)info.Length / (float)(1024 * 1024) * 100) / 100f + " mb";

        ps_useBeatCubeSoundToggle.isOn = ss.prefs.GetBool("useBeatCubeSound");

        StartCoroutine(HandleLoadedProject());

        Rect swipeSize = swipeEditorLocker.GetComponent<RectTransform>().rect;
        swipeEditorLocker.transform.GetChild(1).GetComponent<GridLayoutGroup>().cellSize = new Vector3(swipeSize.width / 4f, swipeSize.height / 2f, 0);
    }

    IEnumerator HandleLoadedProject()
    {
        projectNameText.text = project.author + " - " + project.name;
        if (project.hasImage)
        {
            projectCoverImage.sprite = LoadSprite(project.image);
            projectCoverImage.color = Color.white;
        }


        foreach (BeatCubeClass beat in project.beatCubeList)
        {
            GameObject go;
            if (beat.type == BeatCubeClass.Type.Line)
            {
                go = Instantiate(beatLinePrefab);
            }
            else
            {
                go = Instantiate(beatCubePrefab);
            }

            go.GetComponent<BeatCubeGO>().beatCubeClass = beat;
            go.GetComponent<BeatCubeGO>().OnStart();
            go.GetComponent<BeatCubeGO>().Render();
        }


        if (!File.Exists(Application.persistentDataPath + "/TempFiles/AudioFiles/" + project.author + "-" + project.name + ".ogg"))
        {
            File.WriteAllBytes(Application.persistentDataPath + "/TempFiles/AudioFiles/" + project.author + "-" + project.name + ".ogg", project.audioFile);
        }
        using (WWW www = new WWW("file:///" + Application.persistentDataPath + "/TempFiles/AudioFiles/" + project.author + "-" + project.name + ".ogg"))
        {
            yield return www;
            aSource.clip = www.GetAudioClip();
            aSource.Play();
            timelineSlider.maxValue = aSource.clip.length;
            timeSlider.maxValue = aSource.clip.length;
            if (project.mins == 0 && project.secs == 0)
            {
                project.mins = Mathf.FloorToInt(aSource.clip.length / 60);
                project.secs = Mathf.FloorToInt(aSource.clip.length - project.mins * 60);
            }
        }
    }







    BeatCubeGO selectedBeatCubeGO = null;
    private void Update()
    {
        fpsText.text = Mathf.RoundToInt(1f / Time.smoothDeltaTime) + " fps";

        if (aSource.clip == null) return;

        timeSlider.value = aSource.time;
        timeText.text = SplitTime(aSource.time) + " / " + SplitTime(aSource.clip.length);

        if (pss.isPlaying) return;

        transform.position = new Vector3(transform.position.x, transform.position.y, aSource.time * 20 - 8);
        CamMove.transform.position = new Vector3(CamMove.transform.position.x, CamMove.transform.position.y, transform.position.z + 8);
        timelineSlider.value = aSource.time;

        float mins = Mathf.FloorToInt(aSource.time / 60);
        float secs = Mathf.FloorToInt(aSource.time - mins * 60);
        float mills = aSource.time - mins * 60 - secs;

        timeTMP.text = mins + (secs < 10 ? ":0" + secs : ":" + secs);
        miliTimeTMP.text = mills.ToString().Substring(0);


        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject(-1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 150))
            {
                BeatCubeGO beat = hit.transform.GetComponent<BeatCubeGO>() != null ? hit.transform.GetComponent<BeatCubeGO>() : hit.transform.parent.GetComponent<BeatCubeGO>() != null ? hit.transform.parent.GetComponent<BeatCubeGO>() : null;
                if (beat != null)
                {
                    if (mode_magnet)
                    {
                        OnMagnetSelect(beat);
                    }
                    else if (mode_copypaste)
                    {
                        OnCopyPasteSelect(beat);
                    }
                    else
                    {
                        if (selectedBeatCubeGO != null)
                        {
                            if(selectedBeatCubeGO.beatCubeClass.type != BeatCubeClass.Type.Line)
                            {
                                selectedBeatCubeGO.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
                                selectedBeatCubeGO.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
                                selectedBeatCubeGO.Render();
                            }
                            else
                            {
                                //selectedBeatCubeGO.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", Color.white);
                                selectedBeatCubeGO.Render();
                            }
                        }
                        selectedBeatCubeGO = beat;
                        if (selectedBeatCubeGO.beatCubeClass.type != BeatCubeClass.Type.Line)
                        {
                            selectedBeatCubeGO.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", new Color32(255, 123, 0, 255));
                            selectedBeatCubeGO.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", new Color32(255, 123, 0, 255));
                        }
                        else
                        {
                            selectedBeatCubeGO.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", new Color32(255, 123, 0, 255));
                        }
                        OnModifySelect();
                    }
                }
            }
            //else if(selectedBeatCubeGO != null)
            //{
            //    selectedBeatCubeGO.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
            //    selectedBeatCubeGO.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
            //    selectedBeatCubeGO = null;
            //}
        }
        if(selectedBeatCubeGO == null)
        {
            modify_dirDropdown.transform.parent.gameObject.SetActive(false);
        }
        instrumentTab.GetChild(1).GetComponent<Button>().interactable = selectedBeatCubeGO != null;

        magnetBtns[0].interactable = selectedBeatCubeGO != null;
        magnetBtns[1].gameObject.SetActive(mode_magnet);
        forceToBtn.interactable = selectedBeatCubeGO != null;

        mode_copypasteButtons[0].interactable = selectedBeatCubeGO != null;
        mode_copypasteButtons[1].gameObject.SetActive(mode_copypaste);

        AnimateRoads();

        OnCopyPasteUpdate();

        HotkeyHandler();

        if(swipeEditorLocker.activeSelf) HandleSwipeEditor();
    }
    public void HotkeyHandler()
    {
        if (Input.GetKeyDown(KeyCode.Delete)) DeleteCube();
        if (Input.GetKeyDown(KeyCode.Space)) PlayOrPause();
        if (Input.GetKeyDown(KeyCode.Alpha1)) tap_roadDropdown.value = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) tap_roadDropdown.value = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) tap_roadDropdown.value = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) tap_roadDropdown.value = 3;
        if (Input.GetKeyDown(KeyCode.Tab)) tap_typeDropdown.value = tap_typeDropdown.value == 0 ? 1 : 0;
        if (Input.GetKeyDown(KeyCode.A)) StepClick(-1);
        if (Input.GetKeyDown(KeyCode.D)) StepClick(1);
        if (Input.GetKeyDown(KeyCode.F)) { ForceTo(); }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) { if (!mode_copypaste) { CopyPasteClick(); } else { OnCopyPasteCopy(); } }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V)) { if (mode_copypaste) { OnCopyPastePaste(); } }
    }

    public void AnimateRoads()
    {
        foreach(Transform t in roads)
        {
            MeshRenderer renderer = t.GetComponent<MeshRenderer>();
            Color color = renderer.material.GetColor("_EmissionColor");
            if(color != Color.black)
            {
                renderer.material.SetColor("_EmissionColor", color / 1.5f);
            }
        }
    }


    // =====================================================================================================
    // Инструмент создания кубов по свайпу
    Vector3[] swipesPositions = new Vector3[10];
    int swipeEditorInputDelay;
    public List<Touch> touches;
    Vector3 cursorPizdec;
    void HandleSwipeEditor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            swipeEditorLocker.SetActive(!swipeEditorLocker.activeSelf);
            swipeEditorInputDelay = -1;
            return;
        }
        if (!swipeEditorLocker.activeSelf) return;


        foreach (Transform panel in swipeEditorLocker.transform.GetChild(1))
        {
            panel.GetComponent<Image>().color = new Color32(255, 255, 255, 40);
        }

        if (swipeEditorInputDelay < 5)
        {
            swipeEditorInputDelay++;
        }
        else
        {
            if (Application.isEditor)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    cursorPizdec = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    HandleSwipeEditorLine(cursorPizdec, Input.mousePosition);
                }
            }

            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    HandleSwipeEditorLine(swipesPositions[t.fingerId], t.position);
                }
                else if (t.phase == TouchPhase.Began)
                {
                    swipesPositions[t.fingerId] = t.position;
                }
            }
        }
    }
    void HandleSwipeEditorLine(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        Vector3 swipeSize = new Vector3(Screen.width, Screen.height);
        float road = start.x / swipeSize.x;
        road = road >= 0.75f ? 3 : road >= 0.5f ? 2 : road >= 0.25f ? 1 : 0;
        float level = start.y / swipeSize.y;
        level = level >= 0.5f ? 1 : 0;
        swipeEditorLocker.transform.GetChild(1).GetChild((1 - (int)level) * 4 + (int)road).GetComponent<Image>().color = new Color32(255, 180, 0, 120);

        if (dir == Vector3.zero) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Point, BeatCubeClass.SubType.Random);
        else if (dir.normalized.y <= -0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Down);
        else if (dir.normalized.y >= 0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Up);
        else if (dir.normalized.x <= -0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Left);
        else if (dir.normalized.x >= 0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Right);
    }
    void SpawnCube(int road, int level, BeatCubeClass.Type type, BeatCubeClass.SubType subtype)
    {

        BeatCubeClass beatCube = new BeatCubeClass(aSource.time, road, type);
        beatCube.subType = subtype;
        beatCube.road = road;
        beatCube.level = level;
        beatCube.saberType = road <= 1 ? -1 : 1;
        project.beatCubeList.Add(beatCube);

        GameObject go = Instantiate(beatCubePrefab);
        bcubesHistory.Add(go);
        go.GetComponent<BeatCubeGO>().beatCubeClass = beatCube;
        go.GetComponent<BeatCubeGO>().OnStart();
        go.GetComponent<BeatCubeGO>().Render();
    }

    // =====================================================================================================
    // Инструмент создания кубов по тапу
    public GameObject beatCubePrefab, beatLinePrefab;
    public Dropdown tap_roadDropdown, tap_typeDropdown;
    public Transform[] roads;
    public void CreateObjByTap()
    {
        if(tap_typeDropdown.value == 3)
        {
            selectedBeatCubeGO = CreateLineByTap();
            AddLinePoint();
            AddLinePoint();
            tap_roadDropdown.transform.parent.gameObject.SetActive(false);
            modifyLineInstrument.SetActive(true);
        }
        else
        {
            CreateCubeByTap();
        }
    }
    public void CreateCubeByTap()
    {
        BeatCubeClass beatCube = new BeatCubeClass(aSource.time, tap_roadDropdown.value == 4 ? -1 : tap_roadDropdown.value, tap_typeDropdown.value == 0 ? BeatCubeClass.Type.Point : BeatCubeClass.Type.Dir);
        beatCube.subType = tap_typeDropdown.value == 2 ? BeatCubeClass.SubType.Random : BeatCubeClass.SubType.Down;
        project.beatCubeList.Add(beatCube);

        GameObject go = Instantiate(beatCubePrefab);
        bcubesHistory.Add(go);
        go.GetComponent<BeatCubeGO>().beatCubeClass = beatCube;
        go.GetComponent<BeatCubeGO>().OnStart();
        go.GetComponent<BeatCubeGO>().Render();
    }
    public BeatCubeGO CreateLineByTap()
    {
        BeatCubeClass beatCube = new BeatCubeClass(aSource.time, tap_roadDropdown.value == 4 ? -1 : tap_roadDropdown.value, BeatCubeClass.Type.Line, new Vector3[0]);
        project.beatCubeList.Add(beatCube);

        GameObject go = Instantiate(beatLinePrefab);
        bcubesHistory.Add(go);
        go.GetComponent<BeatCubeGO>().beatCubeClass = beatCube;
        go.GetComponent<BeatCubeGO>().OnStart();
        go.GetComponent<BeatCubeGO>().Render();
        return go.GetComponent<BeatCubeGO>();
    }




    public void OpenModifyInstrument()
    {
        if(selectedBeatCubeGO != null)
        {
            if(selectedBeatCubeGO.beatCubeClass.type == BeatCubeClass.Type.Line)
            {
                modifyLineInstrument.SetActive(true);
            }
            else
            {
                modifyInstrument.SetActive(true);
            }
        }
    }
    // =====================================================================================================
    // Инструмент модификации кубов
    [Header("Модификатор кубов")]
    public GameObject modifyInstrument;
    public InputField modify_mins, modify_secs, modify_mills;
    public Dropdown modify_typeDropdown, modify_dirDropdown, modify_roadDropdown, modify_levelDropdown, modify_saberType;
    public bool onModifySelectDelay;
    public void OnModifyChange()
    {
        InputField if_mins, if_secs, if_mills;
        if(selectedBeatCubeGO.beatCubeClass.type == BeatCubeClass.Type.Line){
            if_mins = modifyLine_mins;
            if_secs = modifyLine_secs;
            if_mills = modifyLine_mills;
        }
        else
        {
            if_mins = modify_mins;
            if_secs = modify_secs;
            if_mills = modify_mills;
        }

        if (onModifySelectDelay)
        {
            return;
        }
        if (selectedBeatCubeGO != null)
        {

            if(int.TryParse(if_mins.text, out int mins)) { if_mins.GetComponent<Image>().color = new Color32(98, 98, 98, 255); }
            else { if_mins.GetComponent<Image>().color = Color.red * 0.5f; return; }

            if (int.TryParse(if_secs.text, out int secs)) { if_secs.GetComponent<Image>().color = new Color32(98, 98, 98, 255); }
            else { if_secs.GetComponent<Image>().color = Color.red * 0.5f; return; }

            if (float.TryParse("0," + if_mills.text, out float mills)) { if_mills.GetComponent<Image>().color = new Color32(98, 98, 98, 255); }
            else { if_mills.GetComponent<Image>().color = Color.red * 0.5f; return; }

            selectedBeatCubeGO.beatCubeClass.time = mins * 60 + secs + mills;

            if(selectedBeatCubeGO.beatCubeClass.type != BeatCubeClass.Type.Line)
            {
                selectedBeatCubeGO.beatCubeClass.type = modify_typeDropdown.value == 0 ? BeatCubeClass.Type.Point : BeatCubeClass.Type.Dir;
                selectedBeatCubeGO.beatCubeClass.subType = modify_dirDropdown.value == 0 ? BeatCubeClass.SubType.Up :
                    modify_dirDropdown.value == 1 ? BeatCubeClass.SubType.Down :
                    modify_dirDropdown.value == 2 ? BeatCubeClass.SubType.Left :
                    modify_dirDropdown.value == 3 ? BeatCubeClass.SubType.Right :
                    BeatCubeClass.SubType.Random;
                selectedBeatCubeGO.beatCubeClass.road = modify_roadDropdown.value == 4 ? -1 : modify_roadDropdown.value;
                selectedBeatCubeGO.beatCubeClass.level = modify_levelDropdown.value;
                selectedBeatCubeGO.beatCubeClass.saberType = modify_saberType.value - 1;

                modify_dirDropdown.interactable = modify_typeDropdown.value == 1;
            }
            
            selectedBeatCubeGO.Render();
        }
    }
    public void DeleteCube()
    {
        if(selectedBeatCubeGO != null)
        {
            project.beatCubeList.Remove(selectedBeatCubeGO.beatCubeClass);
            Destroy(selectedBeatCubeGO.gameObject);
        }
    }

    public void OnModifySelect()
    {
        onModifySelectDelay = true;
        float mins = Mathf.FloorToInt(selectedBeatCubeGO.beatCubeClass.time / 60);
        float secs = Mathf.FloorToInt(selectedBeatCubeGO.beatCubeClass.time - mins * 60);
        float mills = selectedBeatCubeGO.beatCubeClass.time - mins * 60 - secs;

        if (selectedBeatCubeGO.beatCubeClass.type == BeatCubeClass.Type.Line)
        {
            modifyLine_mins.text = mins.ToString();
            modifyLine_secs.text = secs.ToString();
            modifyLine_mills.text = mills.ToString().Replace("0,", "");

            ModifyLineRefreshPointsList();
        }
        else
        {
            modify_mins.text = mins.ToString();
            modify_secs.text = secs.ToString();
            modify_mills.text = mills.ToString().Replace("0,", "");

            modify_typeDropdown.value = selectedBeatCubeGO.beatCubeClass.type == BeatCubeClass.Type.Point ? 0 : 1;
            modify_dirDropdown.value = selectedBeatCubeGO.beatCubeClass.subType == BeatCubeClass.SubType.Up ? 0 :
                selectedBeatCubeGO.beatCubeClass.subType == BeatCubeClass.SubType.Down ? 1 :
                selectedBeatCubeGO.beatCubeClass.subType == BeatCubeClass.SubType.Left ? 2 :
                3;
            modify_roadDropdown.value = selectedBeatCubeGO.beatCubeClass.road;
            modify_levelDropdown.value = selectedBeatCubeGO.beatCubeClass.level;
            modify_saberType.value = selectedBeatCubeGO.beatCubeClass.saberType + 1;

            modify_dirDropdown.interactable = modify_typeDropdown.value == 1;
        }

        onModifySelectDelay = false;
    }
    // =================================================================================================================================================
    // Инструмент модификации линий
    [Header("Модификатор линий")]
    public GameObject modifyLineInstrument;
    public Transform modifyLine_pointsContent;
    public GameObject modifyLine_pointItemPrefab;
    public GameObject modifyLine_editPointPanel;
    public InputField modifyLine_mins, modifyLine_secs, modifyLine_mills;
    public InputField modifyLinePoint_mins, modifyLinePoint_secs, modifyLinePoint_mills;
    public Dropdown modifyLine_roadDropdown;
    public void AddLinePoint()
    {
        if(selectedBeatCubeGO != null)
        {
            float prevZ = -1;
            int length = selectedBeatCubeGO.beatCubeClass.linePoints.ToArray().Length;
            if (length != 0)
            {
                prevZ = selectedBeatCubeGO.beatCubeClass.linePoints[length - 1].z;
            }
            selectedBeatCubeGO.beatCubeClass.linePoints.Add(new Vector3(0, 0, prevZ + 1));

            selectedBeatCubeGO.Render();

            ModifyLineRefreshPointsList();
        }
    }
    public void ModifyLineRefreshPointsList()
    {
        if (selectedBeatCubeGO == null && selectedBeatCubeGO.beatCubeClass.type != BeatCubeClass.Type.Line) return;

        foreach(Transform child in modifyLine_pointsContent)
        {
            Destroy(child.gameObject);
        }

        float height = modifyLine_pointItemPrefab.GetComponent<RectTransform>().sizeDelta.y;
        float contentHeight = 0;
        for (int i = 0; i < selectedBeatCubeGO.beatCubeClass.linePoints.Count; i++)
        {
            GameObject item = Instantiate(modifyLine_pointItemPrefab, modifyLine_pointsContent);
            item.name = i.ToString();

            float pointTime = selectedBeatCubeGO.beatCubeClass.time + selectedBeatCubeGO.beatCubeClass.linePoints[i].z;
            float mins = Mathf.FloorToInt(pointTime / 60);
            float secs = Mathf.FloorToInt(pointTime - mins * 60);
            float mills = pointTime - mins * 60 - secs;

            item.transform.GetChild(0).GetComponent<Text>().text = string.Format("Time: {0}m {1}s {2}ms", mins, secs, mills) + @"
Index: " + i;
        }
        contentHeight = selectedBeatCubeGO.beatCubeClass.linePoints.Count * (height + 2);

        modifyLine_pointsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(10, contentHeight);
    }
    public void ModifyLineEditPoint(GameObject self)
    {
        modifyLine_selectedIndex = int.Parse(self.transform.name);

        float pointTime = selectedBeatCubeGO.beatCubeClass.time + selectedBeatCubeGO.beatCubeClass.linePoints[modifyLine_selectedIndex].z;
        float mins = Mathf.FloorToInt(pointTime / 60);
        float secs = Mathf.FloorToInt(pointTime - mins * 60);
        float mills = pointTime - mins * 60 - secs;
        modifyLinePoint_mins.text = mins.ToString();
        modifyLinePoint_secs.text = secs.ToString();
        modifyLinePoint_mills.text = mills.ToString().Replace("0,", "");

        modifyLine_roadDropdown.value = Mathf.FloorToInt(selectedBeatCubeGO.beatCubeClass.linePoints[modifyLine_selectedIndex].x);

        modifyLine_editPointPanel.SetActive(true);
    }
    public int modifyLine_selectedIndex;
    public void OnModifyLineEditTime()
    {
        if (int.TryParse(modifyLinePoint_mins.text, out int mins)) { modifyLinePoint_mins.GetComponent<Image>().color = new Color32(98, 98, 98, 255); }
        else { modifyLinePoint_mins.GetComponent<Image>().color = Color.red * 0.5f; return; }

        if (int.TryParse(modifyLinePoint_secs.text, out int secs)) { modifyLinePoint_secs.GetComponent<Image>().color = new Color32(98, 98, 98, 255); }
        else { modifyLinePoint_secs.GetComponent<Image>().color = Color.red * 0.5f; return; }

        if (float.TryParse("0," + modifyLinePoint_mills.text, out float mills)) { modifyLinePoint_mills.GetComponent<Image>().color = new Color32(98, 98, 98, 255); }
        else { modifyLinePoint_mills.GetComponent<Image>().color = Color.red * 0.5f; return; }

        selectedBeatCubeGO.beatCubeClass.linePoints[modifyLine_selectedIndex] = new Vector3(
            modifyLine_roadDropdown.value - selectedBeatCubeGO.beatCubeClass.road,
            selectedBeatCubeGO.beatCubeClass.linePoints[modifyLine_selectedIndex].y,
            (mins * 60 + secs + mills) - selectedBeatCubeGO.beatCubeClass.time);

        // При изменении первой точки, pivot LineRenderer'а будет привязан к первой точке
        float diff = selectedBeatCubeGO.beatCubeClass.linePoints[0].z;
        if (diff != 0)
        {
            Debug.Log("Diff: " + diff);

            selectedBeatCubeGO.beatCubeClass.time += diff;

            for (int i = 0; i < selectedBeatCubeGO.beatCubeClass.linePoints.Count; i++)
            {
                Vector3 v3 = selectedBeatCubeGO.beatCubeClass.linePoints[i];
                selectedBeatCubeGO.beatCubeClass.linePoints[i] = new SerializableVector3(v3.x, v3.y, v3.z - diff);
            }
        }

        selectedBeatCubeGO.Render();
    }
    public void MakeSilk()
    {
        if (selectedBeatCubeGO.beatCubeClass.linePoints.Count <= 2)
        {
            modifyLineInstrument.SetActive(false);
            DeleteCube();
            selectedBeatCubeGO = null;
        }
        else
        {
            selectedBeatCubeGO.beatCubeClass.linePoints.RemoveAt(modifyLine_selectedIndex);

            ModifyLineRefreshPointsList();

            selectedBeatCubeGO.Render();
        }
    }

    public float[] GetTime(float allTime)
    {
        float mins = Mathf.FloorToInt(allTime / 60);
        float secs = Mathf.FloorToInt(allTime - mins * 60);
        float mills = allTime - mins * 60 - secs;
        return new float[3] { mins, secs, mills };
    }


    // =================================================================================================================================================
    // Инструмент UNDO
    // =================================================================================================================================================
    public void UndoBeatCube()
    {
        if(bcubesHistory.Count > 0)
        {
            Destroy(bcubesHistory[bcubesHistory.Count - 1]);
            bcubesHistory.RemoveAt(bcubesHistory.Count - 1);
            project.beatCubeList.RemoveAt(project.beatCubeList.Count - 1);
        }
    }


    // =================================================================================================================================================
    // Левый бар
    // =================================================================================================================================================
    // Магнит
    [Header("Левый бар")]
    public List<BeatCubeGO> toMagnetList;
    public bool mode_magnet;
    public void OnMagnetClick()
    {
        mode_magnet = !mode_magnet;
        if (mode_magnet)
        {
            toMagnetList = new List<BeatCubeGO>();
        }
        else
        {
            foreach(BeatCubeGO beat in toMagnetList)
            {
                beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
                beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
            }
            toMagnetList.Clear();
        }
    }
    public void OnMagnetSelect(BeatCubeGO beat)
    {
        if (toMagnetList.Contains(beat))
        {
            toMagnetList.Remove(beat);
            beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
            beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
        }
        else
        {
            toMagnetList.Add(beat);
            beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.magenta);
            beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.magenta);
        }
    }
    public void OnMagnetApply()
    {
        foreach(BeatCubeGO beat in toMagnetList)
        {
            beat.beatCubeClass.time = selectedBeatCubeGO.beatCubeClass.time;
            beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
            beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
            beat.Render();
        }
        mode_magnet = false;
        toMagnetList.Clear();
    }
    // =================================================================================================================================================
    // ForceTo
    public void ForceTo()
    {
        aSource.time = selectedBeatCubeGO.beatCubeClass.time;
    }
    // =================================================================================================================================================
    // CopyPaste
    [Header("  -> CopyPaste")]
    public List<BeatCubeGO> mode_copypasteList;
    public Button[] mode_copypasteButtons;
    public bool mode_copypaste;
    public void CopyPasteClick()
    {
        mode_copypaste = !mode_copypaste;
        if (mode_copypaste)
        {
            mode_copypasteList = new List<BeatCubeGO>();
        }
        else
        {
            if(mode_copypasteList.Count > 0)
            {
                foreach (BeatCubeGO beat in mode_copypasteList)
                {
                    beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
                    beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
                }
                mode_copypasteList.Clear();
            }
        }
    }
    public void OnCopyPasteSelect(BeatCubeGO beat)
    {
        if (mode_copypasteList.Contains(beat))
        {
            mode_copypasteList.Remove(beat);

            if(beat.beatCubeClass.type == BeatCubeClass.Type.Line)
            {
                beat.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", Color.white);
            }
            else
            {
                beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
                beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
            }
        }
        else
        {
            mode_copypasteList.Add(beat);
            if(beat.beatCubeClass.type == BeatCubeClass.Type.Line)
            {
                beat.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", Color.magenta);
            }
            else
            {
                beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.magenta);
                beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.magenta);
            }
        }
    }
    public List<BeatCubeGO> mode_copypasteCopiedList;
    public List<float> mode_copypasteCopiedOffsets;
    public void OnCopyPasteCopy()
    {
        Debug.Log("Copy");
        mode_copypasteCopiedList = new List<BeatCubeGO>();
        mode_copypasteCopiedOffsets = new List<float>();

        mode_copypasteList.Add(selectedBeatCubeGO);
        float offsetToAlign = selectedBeatCubeGO.beatCubeClass.time - aSource.time;

        foreach (BeatCubeGO beat in mode_copypasteList)
        {
            //mode_copypasteCopiedOffsets.Add(beat.beatCubeClass.time - offsetToAlign);
            mode_copypasteCopiedOffsets.Add(beat.beatCubeClass.time - aSource.time);

            GameObject beatGo = Instantiate(beat.gameObject);
            mode_copypasteCopiedList.Add(beatGo.GetComponent<BeatCubeGO>());
            
            if(beat.beatCubeClass.type == BeatCubeClass.Type.Line)
            {
                beatGo.GetComponent<LineRenderer>().material.SetColor("_EmissionColor", Color.magenta);
            }
            else
            {
                beatGo.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.magenta);
                beatGo.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.magenta);

                beat.gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
                beat.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
            }
        }

        mode_copypasteButtons[1].gameObject.SetActive(false);
        mode_copypasteButtons[2].gameObject.SetActive(true);
    }
    public void OnCopyPastePaste()
    {
        Debug.Log("Paste");
        for (int i = 0; i < mode_copypasteCopiedList.ToArray().Length; i++)
        {
            mode_copypasteCopiedList[i].beatCubeClass.time = aSource.time + mode_copypasteCopiedOffsets[i];
            //mode_copypasteCopiedList[i].Render();

            project.beatCubeList.Add(mode_copypasteCopiedList[i].beatCubeClass);

            if (mode_copypasteCopiedList[i].beatCubeClass.type == BeatCubeClass.Type.Line)
            {
                mode_copypasteCopiedList[i].GetComponent<LineRenderer>().material.SetColor("_EmissionColor", Color.white);
            }
            else
            {
                mode_copypasteCopiedList[i].gameObject.GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", Color.white);
                mode_copypasteCopiedList[i].gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_EmissionColor", Color.red);
            }
        }
        mode_copypasteCopiedList.Clear();
        mode_copypaste = false;
        mode_copypasteButtons[1].gameObject.SetActive(false);
        mode_copypasteButtons[2].gameObject.SetActive(false);
    }
    public void OnCopyPasteUpdate()
    {
        if(mode_copypasteCopiedList != null & mode_copypasteCopiedList.Count > 0)
        {
            for (int i = 0; i < mode_copypasteCopiedList.ToArray().Length; i++)
            {
                mode_copypasteCopiedList[i].beatCubeClass.time = aSource.time + mode_copypasteCopiedOffsets[i];
                mode_copypasteCopiedList[i].Render();
            }
        }
    }





    // =====================================================================================================
    // Таймлайн
    [Header("Таймлайн")]
    public Sprite[] playOrPauseSprites;
    public Image playOrPauseImage;
    public void PlayOrPause()
    {
        if(playOrPauseImage.sprite == playOrPauseSprites[0])
        {
            playOrPauseImage.sprite = playOrPauseSprites[1];
            aSource.Pause();
            // pause
        }
        else
        {
            playOrPauseImage.sprite = playOrPauseSprites[0];
            aSource.Play();
            //aSource.UnPause();
            // play
        }
    }
    public void OnTimelineValueChange()
    {
        aSource.time = timelineSlider.value;
    }
    public void StepClick(float dir)
    {
        string str = timeStepField.text;
        if (float.TryParse(str, out float stepValue))
        {
            timeStepField.GetComponent<Image>().color = new Color32(98, 98, 98, 255);
            float step = stepValue * dir;
            float newTime = aSource.time + step;

            if (newTime < 0) newTime = 0;
            else if (newTime > aSource.clip.length) newTime = aSource.clip.length;

            aSource.time = newTime;
        }
        else
        {
            timeStepField.GetComponent<Image>().color = Color.red * .5f;
        }
    }
    public void GoToTime()
    {
        if(int.TryParse(goToTimeFieldsParent.GetChild(0).GetComponent<InputField>().text, out int mins) &&
            int.TryParse(goToTimeFieldsParent.GetChild(1).GetComponent<InputField>().text, out int secs) &&
            float.TryParse("0," + goToTimeFieldsParent.GetChild(2).GetComponent<InputField>().text, out float mills))
        {
            goToTimeFieldsParent.GetChild(3).GetComponent<Text>().text = "";
            aSource.time = mins * 60 + secs + mills;
        }
        else
        {
            goToTimeFieldsParent.GetChild(3).GetComponent<Text>().text = "Wrong time format (Examples: 1:27.9718)";
        }
    }





    // =====================================================================================================
    // Сохранение проекта
    public void QuitWithoutSave()
    {
        SceneManager.LoadScene(0);
    }
    public void SaveAndQuit()
    {
        SaveProject();
        QuitWithoutSave();
    }
    public void SaveProject()
    {
        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create(Application.persistentDataPath + "/Projects/" + project.author + "-" + project.name + ".bsp"))
        {
            binaryFormatter.Serialize(fileStream, project);
        }
    }



    public Sprite LoadSprite(byte[] bytes)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(bytes);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);

        return NewSprite;
    }
    public Texture2D LoadTexture(byte[] bytes)
    {
        Texture2D Tex2D;
        Tex2D = new Texture2D(2, 2);
        if (Tex2D.LoadImage(bytes))
            return Tex2D;
        return null;
    }

    /* ==== Project settings ================================================================================================= */
    public void OnChangePImgClick()
    {
        explorer.Open(Application.persistentDataPath, ChangePImgCallback, ".jpg");
    }
    void ChangePImgCallback(string filepath)
    {
        byte[] imageBytes = File.ReadAllBytes(filepath);
        project.image = imageBytes;
        project.hasImage = true;
        projectCoverImage.sprite = LoadSprite(imageBytes);
        projectCoverImage.color = Color.white;
    }
    public void RenameProject(InputField field)
    {
        if (!field.text.Contains("-")) return;

        project.author = field.text.Split('-')[0];
        project.name = field.text.Split('-')[1];
        projectNameText.text = project.author + " - " + project.name;
    }


    public PlaySongScript pss;
    public void PlayProject()
    {
        pss.Play(project);
    }

    // Просто для удобства и красоты кода
    public string SplitTime(float allTime)
    {
        int mins = Mathf.FloorToInt(allTime / 60f);
        int secs = Mathf.FloorToInt(allTime - mins * 60);
        return mins + ":" + (secs < 10 ? "0" + secs : secs + "");
    }
}


public static class LCData
{
    public static string loadingProjectName = "";
    public static Project project;
    public static AudioClip audioClip;
}