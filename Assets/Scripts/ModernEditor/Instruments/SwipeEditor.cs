using System;
using System.Collections;
using System.Collections.Generic;
using InGame.Game.Spawn;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeEditor : MonoBehaviour
{
    public EditorBeatManager bm;
    
    private Vector3 prevMousePos;
    private Vector3[] swipesPositions = new Vector3[10];
    private float[] swipesTimeings = new float[10];

    public Transform locker;

    private void Update()
    {
        if (Application.isEditor)
        {
            if (IsPointerOverLocker(Input.mousePosition))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    prevMousePos = Input.mousePosition;
                    swipesTimeings[0] = bm.asource.time;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    HandleSwipeEditorLine(prevMousePos, Input.mousePosition, swipesTimeings[0]);
                }   
            }
        }

        foreach (Touch t in Input.touches)
        {
            if (IsPointerOverLocker(Input.mousePosition))
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    HandleSwipeEditorLine(swipesPositions[t.fingerId], t.position, swipesTimeings[t.fingerId]);
                }
                else if (t.phase == TouchPhase.Began)
                {
                    swipesPositions[t.fingerId] = t.position;
                    swipesTimeings[t.fingerId] = bm.asource.time;
                }   
            }
        }
    }
    void HandleSwipeEditorLine(Vector3 start, Vector3 end, float time)
    {
        Vector3 dir = end - start;
        Vector3 swipeSize = new Vector3(Screen.width, Screen.height);
        float road = start.x / swipeSize.x;
        road = road >= 0.75f ? 3 : road >= 0.5f ? 2 : road >= 0.25f ? 1 : 0;
        float level = start.y / swipeSize.y;
        level = level >= 0.5f ? 1 : 0;


        Vector3 direction = end - start;



        /*float angle = Mathf.Atan2(direction.y, direction.x);
        float degrees = angle * 180f / Mathf.PI + 90;


        int type = Mathf.RoundToInt(degrees / 45f);*/
        bool isPoint = dir == Vector3.zero;

        float angle = Mathf.Atan2(direction.x, direction.y);
        float degrees = Mathf.Rad2Deg * -angle;
        if (degrees < 0) degrees = 360 + degrees;

        float i = Mathf.RoundToInt(degrees / 45f);
        BeatCubeClass.SubType subtype = (BeatCubeClass.SubType)Mathf.Repeat((int)i + 4, 8);

        SpawnCube((int)road, (int)level, isPoint ? BeatCubeClass.Type.Point : BeatCubeClass.Type.Dir, subtype, time);
    }
    
    private bool IsPointerOverLocker(Vector2 pos) 
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = pos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        
        return results.Count == 0 ? false : results[0].gameObject == locker.gameObject;
    }
    
    

    void SpawnCube(int road, int level, BeatCubeClass.Type type, BeatCubeClass.SubType subType, float time)
    {
        BeatCubeClass cls = new BeatCubeClass()
        {
            time = time,
            road = road,
            level = level,
            saberType = road < 2 ? -1 : 1,
            type = type,
            subType = subType,
            speed = 1
        };
        bm.beatLs.Add(cls);
        bm.SpawnBeatCube(cls);
    }
}