using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Game.Spawn
{
    public class EditorBeatManager : MonoBehaviour
    {
        public ModernEditorManager manager;
        
        public GameObject cubePrefab, linePrefab;
        public Camera cam;
        public AudioSource asource;

        public ModernEditor.Instruments.InspectorTool inspectorTool;

        [Space(25)]
        [Header("BPM")]
        public MeshRenderer beatIndicator;
        public float bpm;
        float beatRangeTime;
        int beatsPassedPrev;

        [Header("Field")]
        public Transform fieldTransform;

        // Beat field stuff
        public float fieldLength, fieldCrossTime;
        public float CubeSpeed
        {
            get { return fieldLength; }
        }
        public float playAreaZ;

        public List<BeatCubeClass> beatLs;


        public void LoadBeats(List<BeatCubeClass> beatLs)
        {
            foreach (Transform beat in fieldTransform)
            {
                beat.GetComponent<IEditorBeat>().Delete();
            }
            
            this.beatLs = new List<BeatCubeClass>();
            this.beatLs.AddRange(beatLs);

            foreach (BeatCubeClass cls in beatLs)
            {
                SpawnBeatCube(cls);
            }
        }



        private void Update()
        {
            fieldTransform.position = new Vector3(0, 0, -fieldLength * asource.time);

            // Show BPM
            if(bpm > 0)
            {
                beatIndicator.material.SetColor("_EmissionColor", ClampColor(beatIndicator.material.GetColor("_EmissionColor") * 0.9f, 0.02f, 1));

                beatRangeTime = 1f / (bpm / 60f);
                int beatsPassed = Mathf.CeilToInt(asource.time / beatRangeTime);
                if (beatsPassedPrev != beatsPassed)
                {
                    if (beatsPassed > beatsPassedPrev)
                    {
                        beatIndicator.material.SetColor("_EmissionColor", new Color(1, 1, 1, 1));
                        Debug.Log("Beat");
                    }
                    beatsPassedPrev = beatsPassed;
                }
            }
            else beatIndicator.material.SetColor("_EmissionColor", ClampColor(beatIndicator.material.GetColor("_EmissionColor") * 0.9f, 0.02f, 0.02f));
        }

        
        public void CalculateField()
        {
            float spawnZ = 100;
            playAreaZ = -20 + 26; // 26 это расстояние от камеры до точки где удобно резать кубы
            float distanceSpawnAndPlayArea = spawnZ - playAreaZ;

            fieldLength = distanceSpawnAndPlayArea; // Длина поля в юнитах (где летят кубы)
            fieldCrossTime = 1; // Время за которое куб должен преодолеть поле (в секундах)

            //float mult = replay.cubesSpeed / replay.musicSpeed; 
            float mult = 1;
            // scale для поля
            // Чем больше игрок поставил скорость кубам, тем .... в жопу, потом разберусь с модами

            //cubeSpeed = fieldLength * Time.deltaTime * mult; // Скорость куба (логично)
        }

        public void SpawnBeatCube(BeatCubeClass cls)
        {
            GameObject go = Instantiate(cls.type == BeatCubeClass.Type.Line ? linePrefab : cubePrefab, fieldTransform);
            IEditorBeat beat = go.GetComponent<IEditorBeat>();
            beat.Setup(cls, this);
        }
        
        

        public void OnPoint(IEditorBeat beat)
        {
            inspectorTool.OnOpen(false);
        }


        public void DeleteBeat(IEditorBeat beat)
        {
            beatLs.Remove(beat.GetClass());
            beat.Delete();
        }
        
        
        
        

        Color ClampColor(Color clr, float min, float max)
        {
            float r = Mathf.Clamp(clr.r, min, max);
            float g = Mathf.Clamp(clr.g, min, max);
            float b = Mathf.Clamp(clr.b, min, max);
            float a = Mathf.Clamp(clr.a, min, max);
            return new Color(r, g, b, a);
        }
    }

    public class SpawnPointClass
    {
        public int index;
        public float cooldown;
    }
}