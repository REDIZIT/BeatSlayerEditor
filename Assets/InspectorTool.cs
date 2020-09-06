using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InEditor.Legacy
{
    public class InspectorTool : MonoBehaviour
    {
        EditorScript editor;
        [HideInInspector] public BeatCubeGO selectedCube;
        [HideInInspector] public bool isMultiSelecting;
        [HideInInspector] public List<BeatCubeGO> selectedCubes = new List<BeatCubeGO>();

        [Header("Инспектор")]
        public Transform inspectorWindow;
        public Button openInspectorBtn;
        public InputField minIF, secIF, msIF;
        public Dropdown roadDrop, typeDrop, dirDrop, heightDrop, saberDrop;
        public Transform pointsGroup;
        public Toggle isCubeLineToggles;
        public ToggleGroup linesRoadGroup;
        public InputField linesTimeField;

        [Header("Спавнер")]
        public GameObject spawnerWindow;
        Transform spawnerBody { get { return spawnerWindow.transform.GetChild(1); } }

        [Header("Закладки")]
        public GameObject bookmarkWindow;
        Transform bookmarkBody { get { return bookmarkWindow.transform.GetChild(1); } }
        public Button removeBookmarkBtn;

        [Header("Свайп редактор")]
        public GameObject swipeEditorLocker;

        private void Awake()
        {
            editor = Camera.main.GetComponent<EditorScript>();
        }

        private void Update()
        {
            if (editor.isCopying) return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit))
            {
                BeatCubeGO go = hit.transform.GetComponent<BeatCubeGO>();
                if (hit.transform.name == "LineCollider") go = hit.transform.parent.GetComponent<BeatCubeGO>();
                if (go != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!isMultiSelecting)
                        {
                            if (selectedCube == go)
                            {
                                selectedCube.OnDeselect();
                                selectedCube = null;
                                editor.OnBeatCubeDeselected();
                                CloseInspectorTool();
                            }
                            else
                            {
                                if (selectedCube != null) selectedCube.OnDeselect();
                                selectedCube = go;
                                go.OnSelect();
                                OpenInspectorTool();
                                editor.OnBeatCubeSelected();
                            }
                        }
                        else
                        {
                            List<BeatCubeGO> cubesInSelectedArray = selectedCubes.Where(c => c.beatCubeClass == go.beatCubeClass).ToList();
                            BeatCubeGO cubeInSelectedArray = cubesInSelectedArray.Count > 0 ? cubesInSelectedArray[0] : null;
                            if (cubeInSelectedArray != null)
                            {
                                cubeInSelectedArray.OnDeselect();
                                selectedCubes.Remove(cubeInSelectedArray);
                                Debug.LogWarning("Removed from arr");
                            }
                            else
                            {
                                go.OnSelect();
                                selectedCubes.Add(go);
                                Debug.LogWarning("Added to arr");
                            }
                        }
                    }
                }
            }

            inspectorWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(inspectorWindow.GetComponent<RectTransform>().sizeDelta.x, inspectorWindow.GetChild(1).GetComponent<VerticalLayoutGroup>().preferredHeight + 60 + 10);
            //inspectorWindow.GetChild(1).GetChild(6).GetComponent<RectTransform>().sizeDelta = new Vector2(inspectorWindow.GetChild(1).GetChild(6).GetComponent<RectTransform>().sizeDelta.x, inspectorWindow.GetChild(1).GetChild(6).GetChild(2).GetComponent<VerticalLayoutGroup>().preferredHeight + 50 + 15);

            openInspectorBtn.interactable = selectedCube != null;

            HandleSwipeEditor();
        }




        #region Инспектор тул

        bool isInspectorIniting;
        public void OpenInspectorTool()
        {
            if (selectedCube == null) return;

            inspectorWindow.gameObject.SetActive(true);
            spawnerWindow.SetActive(false);
            isInspectorIniting = true;

            BeatCubeClass cls = selectedCube.beatCubeClass;
            int[] timeArr = TheGreat.SecondsToInts(cls.time);


            minIF.text = timeArr[0] + "";
            secIF.text = timeArr[1] + "";
            msIF.text = timeArr[2] + "";

            //roadDrop.value = cls.road;

            #region Действия со падом (высота, тип, направление)

            if (cls.type == BeatCubeClass.Type.Point)
            {
                foreach (var comp in directionToggleGroup.GetComponentsInChildren<Toggle>()) comp.interactable = true;
                directionToggleGroup.transform.GetComponentsInChildren<Toggle>().Where(c => c.transform.name == "Point").First().isOn = true;
            }
            else if (cls.type == BeatCubeClass.Type.Dir)
            {
                foreach (var comp in directionToggleGroup.GetComponentsInChildren<Toggle>()) comp.interactable = true;
                directionToggleGroup.transform.GetComponentsInChildren<Toggle>().Where(c => c.transform.name == cls.subType.ToString()).First().isOn = true;
            }
            else
            {
                foreach (var comp in directionToggleGroup.GetComponentsInChildren<Toggle>()) comp.interactable = false;
                isCubeLineToggles.isOn = true;
            }

            heightToggleGroup.transform.GetChild(cls.level).GetComponent<Toggle>().isOn = true;

            roadToggleGroup.transform.GetChild(cls.road).GetComponent<Toggle>().isOn = true;


            #endregion

            saberDrop.value = cls.saberType == -1 ? 0 : 1;

            pointsGroup.gameObject.SetActive(cls.type == BeatCubeClass.Type.Line);
            if (cls.type == BeatCubeClass.Type.Line) RefreshInspectorLinePoints();

            isInspectorIniting = false;
        }
        public void CloseInspectorTool()
        {
            inspectorWindow.gameObject.SetActive(false);
        }
        public void OnDropdownChange(Dropdown drop)
        {
            if (selectedCube == null || isInspectorIniting) return;
            isInspectorIniting = true;

            if (drop.transform.parent.name == "Road")
            {
                selectedCube.beatCubeClass.road = drop.value;
                selectedCube.Render();
            }
            else if (drop.transform.parent.name == "Type")
            {
                selectedCube.beatCubeClass.type = (BeatCubeClass.Type)drop.value;
                dirDrop.transform.parent.gameObject.SetActive((BeatCubeClass.Type)drop.value == BeatCubeClass.Type.Dir);
                selectedCube.Render();
            }
            else if (drop.transform.parent.name == "Height")
            {
                selectedCube.beatCubeClass.level = drop.value;
                selectedCube.Render();
            }
            else if (drop.transform.parent.name == "Saber")
            {
                selectedCube.beatCubeClass.saberType = drop.value == 0 ? -1 : 1;
                selectedCube.Render();
            }

            isInspectorIniting = false;
        }
        public void OnInputfieldChange(InputField field)
        {
            if (selectedCube == null || isInspectorIniting) return;

            if (field.name == "MsField" || field.name == "SecField" || field.name == "MinField")
            {
                if (msIF.text == "" || secIF.text == "" || minIF.text == "") return;

                DataTable dt = new DataTable();
                try
                {
                    var min = dt.Compute(minIF.text, "");
                    var sec = dt.Compute(secIF.text, "");
                    var ms = dt.Compute(msIF.text, "");

                    isInspectorIniting = true;
                    minIF.text = min + "";
                    secIF.text = sec + "";
                    msIF.text = ms + "";
                    isInspectorIniting = false;

                    float allTime = (int)min * 60 + (int)sec + (int)ms / 1000f;

                    selectedCube.beatCubeClass.time = allTime;
                    selectedCube.Render();
                }
                catch (Exception err) { Debug.LogError(err); }
            }
        }
        public void RefreshInspectorLinePoints()
        {
            //Transform body = pointsGroup.GetChild(2);
            //foreach (Transform child in body) if (child.name != "Item") Destroy(child.gameObject);
            //body.GetChild(0).gameObject.SetActive(true);

            //int i = 0;
            //foreach (SerializableVector3 point in selectedCube.beatCubeClass.linePoints)
            //{
            //    Transform item = Instantiate(body.GetChild(0).gameObject, body).transform;
            //    item.GetChild(0).GetComponent<Text>().text = "Точка " + i;

            //    int[] time = Helper.SecondsToInts(point.z);
            //    item.GetChild(1).GetChild(1).GetComponent<InputField>().text = time[0].ToString();
            //    item.GetChild(1).GetChild(2).GetComponent<InputField>().text = time[1].ToString();
            //    item.GetChild(1).GetChild(3).GetComponent<InputField>().text = time[2].ToString();

            //    item.GetChild(2).GetChild(1).GetComponent<Dropdown>().value = (int)point.x;

            //    i++;
            //}
            //body.GetChild(0).gameObject.SetActive(false);
            linesTimeField.text = selectedCube.beatCubeClass.linePoints[1].z * 1000f + "";
            linesRoadGroup.transform.GetChild(selectedCube.beatCubeClass.road).GetComponent<Toggle>().isOn = true;
        }
        public void OnLineInputChange(Transform item)
        {
            if (selectedCube == null || isInspectorIniting) return;
            isInspectorIniting = true;

            Transform timePan = item.GetChild(1);

            int pointId = int.Parse(timePan.parent.GetChild(0).GetComponent<Text>().text.Replace("Точка ", ""));
            float allTime = 0;
            for (int i = 1; i < timePan.childCount; i++)
            {
                int parsed = int.Parse(timePan.GetChild(i).GetComponent<InputField>().text);
                if (timePan.GetChild(i).name == "MsField") allTime += parsed / 1000f;
                if (timePan.GetChild(i).name == "SecField") allTime += parsed;
                if (timePan.GetChild(i).name == "MinField") allTime += parsed * 60;
            }

            int road = item.GetChild(2).GetChild(1).GetComponent<Dropdown>().value;

            SerializableVector3 v3 = new SerializableVector3(road, selectedCube.beatCubeClass.linePoints[pointId].y, allTime);
            selectedCube.beatCubeClass.linePoints[pointId] = v3;
            selectedCube.Render();

            isInspectorIniting = false;
        }

        #region Направление куба

        public ToggleGroup directionToggleGroup;
        public void OnDirectionToggleChanged()
        {
            Toggle toggle = directionToggleGroup.ActiveToggles().First();

            //selectedCube.beatCubeClass.subType = (BeatCubeClass.SubType)drop.value;
            //selectedCube.Render();


            // Перебираем (знаю, можно лучше, но так надёжнее)
            BeatCubeClass.SubType dir =
                toggle.name == "Up" ? BeatCubeClass.SubType.Up :
                toggle.name == "Down" ? BeatCubeClass.SubType.Down :
                toggle.name == "Left" ? BeatCubeClass.SubType.Left :
                toggle.name == "Right" ? BeatCubeClass.SubType.Right :
                toggle.name == "UpLeft" ? BeatCubeClass.SubType.UpLeft :
                toggle.name == "UpRight" ? BeatCubeClass.SubType.UpRight :
                toggle.name == "DownLeft" ? BeatCubeClass.SubType.DownLeft :
                toggle.name == "DownRight" ? BeatCubeClass.SubType.DownRight :
                BeatCubeClass.SubType.Random;

            selectedCube.beatCubeClass.subType = dir;

            if (toggle.name == "Point")
            {
                selectedCube.beatCubeClass.type = BeatCubeClass.Type.Point;
            }
            else if (selectedCube.beatCubeClass.type != BeatCubeClass.Type.Line)
            {
                selectedCube.beatCubeClass.type = BeatCubeClass.Type.Dir;
            }
            else
            {
                selectedCube.beatCubeClass.type = BeatCubeClass.Type.Line;
            }

            selectedCube.Render();
        }

        #endregion

        #region Высота

        public ToggleGroup heightToggleGroup;
        public void OnHeightToggleChanged()
        {
            Toggle toggle = heightToggleGroup.ActiveToggles().First();

            selectedCube.beatCubeClass.level = toggle.name == "Up" ? 1 : 0;
            selectedCube.Render();
        }

        #endregion

        #region Дорога

        public ToggleGroup roadToggleGroup;
        public void OnRoadToggleChanged()
        {
            Toggle toggle = roadToggleGroup.ActiveToggles().First();

            int road = int.Parse(toggle.name) - 1;
            selectedCube.beatCubeClass.road = road;
            selectedCube.Render();
        }

        #endregion

        #region Линии

        public void OnLinesTimeChange()
        {
            int msTime;
            if (int.TryParse(linesTimeField.text, out msTime))
            {
                selectedCube.beatCubeClass.linePoints[1] = new SerializableVector3(selectedCube.beatCubeClass.linePoints[1].x, 0, msTime / 1000f);
                selectedCube.Render();
            }
        }
        public void OnLinesRoadChange(Toggle t)
        {
            if (int.TryParse(linesTimeField.text, out int msTime))
            {
                selectedCube.beatCubeClass.linePoints[1] = new SerializableVector3(int.Parse(t.name) - 1, 0, msTime / 1000f);
                selectedCube.Render();
            }
        }

        #endregion

        #endregion

        #region Спавнер тул

        bool isToolIniting;
        public void OpenSpawnerTool()
        {
            spawnerWindow.SetActive(true);
            inspectorWindow.gameObject.SetActive(false);
        }
        public void OnSpawnerToolChanged()
        {
            Dropdown type = spawnerBody.GetChild(1).GetChild(1).GetComponent<Dropdown>();
            spawnerBody.GetChild(2).gameObject.SetActive(type.value != 2);
        }
        public void OnSpawnerToolSpawn()
        {
            Dropdown road = spawnerBody.GetChild(0).GetChild(1).GetComponent<Dropdown>();
            Dropdown type = spawnerBody.GetChild(1).GetChild(1).GetComponent<Dropdown>();
            Dropdown dir = spawnerBody.GetChild(2).GetChild(1).GetComponent<Dropdown>();
            Dropdown height = spawnerBody.GetChild(3).GetChild(1).GetComponent<Dropdown>();
            BeatCubeClass cls = new BeatCubeClass(editor.aSource.time, road.value, (BeatCubeClass.Type)type.value);
            cls.saberType = cls.road < 2 ? -1 : 1;
            cls.subType = (BeatCubeClass.SubType)dir.value;
            cls.level = height.value;
            cls.linePoints = new List<SerializableVector3>() { new SerializableVector3(0, 0, 0), new SerializableVector3(0, 0, 1) };
            editor.SpawnCube(cls);
        }
        #endregion

        #region Закладки

        Color bookmarkColor = new Color(1, 0, 0);
        public void CreateBookmark()
        {
            int bookmarkType = bookmarkBody.GetChild(0).GetChild(1).GetComponent<Dropdown>().value;
            GameObject go = editor.CreateBookmark(editor.aSource.time, bookmarkColor, bookmarkType);

            Bookmark bookmark = new Bookmark(editor.aSource.time, bookmarkType, bookmarkColor);
            editor.project.bookmarks.Add(bookmark);

            editor.bookmarks.Add(new EditorBookmark(bookmark, go));

            removeBookmarkBtn.interactable = editor.bookmarks.Count > 0;
        }
        public void RemoveBookmark()
        {
            if (editor.bookmarks.Count == 0) return;
            EditorBookmark closest = editor.bookmarks.OrderBy(c => Mathf.Abs(c.bookmark.time - editor.aSource.time)).ToArray()[0];
            editor.RemoveBookmark(closest);

            removeBookmarkBtn.interactable = editor.bookmarks.Count > 0;
        }


        public void OnBookmarkColorChange(Transform color)
        {
            bookmarkColor = color.GetComponent<Image>().color;
            foreach (Transform clr in color.parent)
            {
                clr.GetComponent<Outline>().effectColor = clr.GetComponent<Image>().color == bookmarkColor ? new Color(1, 0.5f, 0, 1) : new Color(0.01f, 0.01f, 0.01f, 1);
            }
        }

        #endregion

        #region Редактор свайпами

        Vector3[] swipesPositions = new Vector3[10];
        int swipeEditorInputDelay;
        public List<Touch> touches;
        Vector3 cursorPizdec;
        void HandleSwipeEditor()
        {
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{

            //    return;
            //}
            if (!swipeEditorLocker.activeSelf) return;

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


            if (dir == Vector3.zero) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Point, BeatCubeClass.SubType.Random);
            else if (dir.normalized.y <= -0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Down);
            else if (dir.normalized.y >= 0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Up);
            else if (dir.normalized.x <= -0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Left);
            else if (dir.normalized.x >= 0.6f) SpawnCube((int)road, (int)level, BeatCubeClass.Type.Dir, BeatCubeClass.SubType.Right);
        }
        void SpawnCube(int road, int level, BeatCubeClass.Type type, BeatCubeClass.SubType sub)
        {
            BeatCubeClass cls = new BeatCubeClass(editor.aSource.time, road, type) { subType = sub, saberType = road < 2 ? -1 : 1, level = level };
            editor.SpawnCube(cls);
        }
        public void CloseSwipeEditor()
        {
            swipeEditorLocker.SetActive(!swipeEditorLocker.activeSelf);
            swipeEditorInputDelay = -1;
        }

        #endregion



        public void HotKeysHandler()
        {
            if (spawnerWindow.activeSelf)
            {


                int alphaValue = Input.GetKeyDown(KeyCode.Alpha1) ? 1 : Input.GetKeyDown(KeyCode.Alpha2) ? 2 : Input.GetKeyDown(KeyCode.Alpha3) ? 3 : Input.GetKeyDown(KeyCode.Alpha4) ? 4 : -1;
                if (alphaValue != -1) spawnerBody.GetChild(0).GetChild(1).GetComponent<Dropdown>().value = alphaValue - 1;

                int dirValue = Input.GetKeyDown(KeyCode.DownArrow) ? 0 : Input.GetKeyDown(KeyCode.LeftArrow) ? 1 : Input.GetKeyDown(KeyCode.RightArrow) ? 2 : Input.GetKeyDown(KeyCode.UpArrow) ? 3 : -1;
                if (dirValue != -1) spawnerBody.GetChild(2).GetChild(1).GetComponent<Dropdown>().value = dirValue;

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    int val = spawnerBody.GetChild(1).GetChild(1).GetComponent<Dropdown>().value == 0 ? 1 : 0;
                    spawnerBody.GetChild(1).GetChild(1).GetComponent<Dropdown>().value = val;
                }

                if (Input.GetKeyDown(KeyCode.E)) OnSpawnerToolSpawn();

                if (Input.GetKeyDown(KeyCode.Escape)) spawnerWindow.SetActive(false);
            }
        }

        public void SpawnBeatCube()
        {
            Dropdown roadDropdown = spawnerBody.GetComponentsInChildren<Dropdown>()[0];
            Dropdown typeDropdown = spawnerBody.GetComponentsInChildren<Dropdown>()[1];
            Dropdown dirDropdown = spawnerBody.GetComponentsInChildren<Dropdown>()[2];
            Dropdown heightDropdown = spawnerBody.GetComponentsInChildren<Dropdown>()[3];

            BeatCubeClass cls = new BeatCubeClass(editor.aSource.time, roadDropdown.value, typeDropdown.value == 0 ? BeatCubeClass.Type.Dir : BeatCubeClass.Type.Point);
            cls.level = heightDropdown.value;
            cls.subType = (BeatCubeClass.SubType)dirDropdown.value;

            editor.SpawnCube(cls);
        }
    }
}