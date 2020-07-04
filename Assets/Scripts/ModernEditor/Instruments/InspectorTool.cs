﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InGame.Game.Spawn;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ModernEditor.Instruments
{
    public class InspectorTool : MonoBehaviour
    {
        public EditorBeatManager bm;

        public BeatCubeClass Cls => mode == ToolMode.Inspector ? selectedCube.GetClass() : spawningCls;

        public BeatCubeClass spawningCls;
        public IEditorBeat selectedCube;
        
        private enum ToolMode { Spawner, Inspector }
        ToolMode mode;
        

        public Animator animator;

        private bool isRefreshing;
        [Header("UI")] 
        public Text label;
        public InputField timeMinField;
        public InputField timeSecField, timeMsField, speedField;
        public ToggleGroup dirGroup, roadGroup, heightGroup;
        public GameObject typeSection, saberSection;
        public ToggleGroup typeGroup, saberGroup;
        public GameObject timeSection, cubeSection, deleteBtn, spawnBtn;

        [Header("Line UI")] 
        public GameObject lineSection;
        public ToggleGroup lineRoadGroup, lineHeightGroup;
        public ToggleGroup lineEndRoadGroup, lineEndHeightGroup;
        public InputField lineEndMins, lineEndSecs, lineEndMs;

        private const float MaxCubeSpeed = 8;


        public void OnOpen(bool isSpawner)
        {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Inspector-open")) animator.Play("Inspector-open");
            
            if (selectedCube == null)
            {
                Refresh(true);
            }
            else
            {
                Refresh(isSpawner);
            }
        }

        public void OnClose()
        {
            animator.Play("Inspector-close");
        }

        private void Refresh(bool isSpawner)
        {
            isRefreshing = true;
            mode = isSpawner ? ToolMode.Spawner : ToolMode.Inspector;

            label.text = isSpawner ? "Spawner" : "Inspector";

            timeSection.SetActive(!isSpawner);
            deleteBtn.SetActive(!isSpawner);
            spawnBtn.SetActive(isSpawner);
            typeSection.SetActive(isSpawner);
            saberSection.SetActive(Cls?.type != BeatCubeClass.Type.Line);


            cubeSection.SetActive(Cls.type == BeatCubeClass.Type.Dir || Cls.type == BeatCubeClass.Type.Point);
            lineSection.SetActive(Cls.type == BeatCubeClass.Type.Line);

            speedField.text = Cls.speed.ToString();
            
            

            // Cube stuff
            IEnumerable<Toggle> dirToggles = dirGroup.transform.GetComponentsInChildren<Toggle>();
            IEnumerable<Toggle> roadToggles = roadGroup.transform.GetComponentsInChildren<Toggle>();
            IEnumerable<Toggle> heightToggles = heightGroup.transform.GetComponentsInChildren<Toggle>();
            IEnumerable<Toggle> saberToggles = saberGroup.transform.GetComponentsInChildren<Toggle>();
            // Line stuff
            IEnumerable<Toggle> lineRoadToggles = lineRoadGroup.transform.GetComponentsInChildren<Toggle>();
            IEnumerable<Toggle> lineHeightToggles = lineHeightGroup.transform.GetComponentsInChildren<Toggle>();
            IEnumerable<Toggle> lineEndRoadToggles = lineEndRoadGroup.transform.GetComponentsInChildren<Toggle>();
            IEnumerable<Toggle> lineEndHeightToggles = lineEndHeightGroup.transform.GetComponentsInChildren<Toggle>();

            
            // === Cube set ===
            // Direction
            if (Cls.type == BeatCubeClass.Type.Point)
            {
                Toggle t = dirToggles.Where(c => c.transform.name == "Point").First();
                t.isOn = true;
            }
            else
            {
                Toggle t = dirToggles.Where(c => c.transform.name == Cls.subType.ToString()).First();
                t.isOn = true;
            }

            // Road, Level
            roadToggles.Where(c => c.transform.name == Cls.road.ToString()).First().isOn = true;
            heightToggles.Where(c => c.transform.name == Cls.level.ToString()).First().isOn = true;

            // Time
            int[] timeInts = TheGreat.SecondsToInts(Cls.time);
            timeMinField.text = timeInts[0].ToString();
            timeSecField.text = timeInts[1].ToString();
            timeMsField.text = timeInts[2].ToString();

            // Saber
            saberToggles.Where(c => c.transform.name == Cls.saberType.ToString()).First().isOn = true;
            
            
            // === Line set ===
            
            // - First point
            SetActiveToggle(lineRoadToggles, Cls.road.ToString());
            SetActiveToggle(lineHeightToggles, Cls.level.ToString());
            // - Second point
            SetActiveToggle(lineEndRoadToggles, Cls.lineEndRoad.ToString());
            SetActiveToggle(lineEndHeightToggles, Cls.lineEndLevel.ToString());
            // - Second point time
            SetTimeInputFields(lineEndMins, lineEndSecs, lineEndMs, Cls.lineLenght);
            

            isRefreshing = false;
        }

        public void OnValueChanged()
        {
            if (isRefreshing) return;
            
            // Time
            if (int.TryParse(timeMinField.text, out int mins) && int.TryParse(timeSecField.text, out int secs) &&
                int.TryParse(timeMsField.text, out int ms))
            {
                Cls.time = TheGreat.IntsToSeconds(new int[3] {mins, secs, ms});
            }

            // Type
            if (mode == ToolMode.Spawner)
            {
                Toggle typeT = GetActiveToggle(typeGroup);
                BeatCubeClass.Type.TryParse(typeT.transform.name, true, out BeatCubeClass.Type type);
                Cls.type = type;
            }
            
            // Speed
            try
            {
                speedField.text = speedField.text.Replace(",", ".");
                float speed = float.Parse(speedField.text, CultureInfo.InvariantCulture.NumberFormat);
                speed = Mathf.Clamp(speed, 0.1f, MaxCubeSpeed);
                
                Cls.speed = speed;
            }
            catch (Exception e)
            {
            }

            
            
            
            // === Cube stuff ===
            if (Cls.type != BeatCubeClass.Type.Line)
            {
                // Dir
                Toggle dirT = GetActiveToggle(dirGroup);
                if (dirT.transform.name == "Point") Cls.type = BeatCubeClass.Type.Point;
                else
                {
                    Cls.type = BeatCubeClass.Type.Dir;
                    BeatCubeClass.SubType.TryParse(dirT.transform.name, true, out BeatCubeClass.SubType rotation);
                    Cls.subType = rotation;
                }
            
                // Road and saber
                Toggle roadT = GetActiveToggle(roadGroup);
                int newRoad = int.Parse(roadT.transform.name);
                Toggle saberT;
                if (Cls.road != newRoad)
                {
                    Cls.road = newRoad;
                    // Saber (change saber based on road)
                    Cls.saberType = Cls.road < 2 ? -1 : 1;
                    SetActiveToggle(saberGroup.transform.GetComponentsInChildren<Toggle>(), Cls.saberType.ToString());
                }
                else
                {
                    // Saber (just refresh)
                    saberT = GetActiveToggle(saberGroup);
                    Cls.saberType = int.Parse(saberT.transform.name);
                }

                // Level
                Toggle heightT = GetActiveToggle(heightGroup);
                Cls.level = int.Parse(heightT.transform.name);
            }
            
            // === Line stuff ===
            if (Cls.type == BeatCubeClass.Type.Line)
            {
                // - First point
                Cls.road = int.Parse(GetActiveToggle(lineRoadGroup).transform.name);
                Cls.level = int.Parse(GetActiveToggle(lineHeightGroup).transform.name);
                // - Second point
                Cls.lineEndRoad = int.Parse(GetActiveToggle(lineEndRoadGroup).transform.name);
                Cls.lineEndLevel = int.Parse(GetActiveToggle(lineEndHeightGroup).transform.name);
                // - Second point time
                Cls.lineLenght = GetTimeInputFields(lineEndMins, lineEndSecs, lineEndMs);
            }


            selectedCube?.Refresh();
        }

        public void OnWindButtonClicked(int direction)
        {
            float newTime = Cls.time + direction * 0.02f;

            Cls.time = newTime;

            selectedCube?.Refresh();

            int[] timeInts = TheGreat.SecondsToInts(Cls.time);
            timeMinField.text = timeInts[0].ToString();
            timeSecField.text = timeInts[1].ToString();
            timeMsField.text = timeInts[2].ToString();
        }




        public void DeleteBeat()
        {
            if (selectedCube == null) return;

            bm.DeleteBeat(selectedCube);
            selectedCube = null;

            OnClose();
        }

        public void SpawnBeat()
        {
            Cls.time = bm.asource.time;
            BeatCubeClass clonedCls = Cls.Clone();
            if (clonedCls.type == BeatCubeClass.Type.Line)
            {
                clonedCls.saberType = 0;
            }
            
            bm.SpawnBeatCube(clonedCls);
            bm.beatLs.Add(clonedCls);
        }

        private Toggle GetActiveToggle(ToggleGroup group)
        {
            foreach (Transform toggle in group.transform)
            {
                if (toggle.GetComponent<Toggle>().isOn) return toggle.GetComponent<Toggle>();
            }

            return null;
        }

        private void SetActiveToggle(IEnumerable<Toggle> group, string name)
        {
            group.Where(c => c.transform.name == name).First().isOn = true;
        }

        private void SetTimeInputFields(InputField minField, InputField secField, InputField msField, float time)
        {
            int[] timeInts = TheGreat.SecondsToInts(time);
            minField.text = timeInts[0].ToString();
            secField.text = timeInts[1].ToString();
            msField.text = timeInts[2].ToString();
        }

        private float GetTimeInputFields(InputField minField, InputField secField, InputField msField)
        {
            int mins = int.Parse(minField.text);
            int secs = int.Parse(secField.text);
            int ms = int.Parse(msField.text);
            return TheGreat.IntsToSeconds(new int[3] {mins, secs, ms});
        }
    }
}