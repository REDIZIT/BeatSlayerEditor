using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatCubeGO : MonoBehaviour
{
    public BeatCubeClass beatCubeClass;
    public int road;
    //SongEditor editor;
    EditorScript editor;

    // Обычная сфера
    public GameObject linePointPrefab;

    public static float unitsRoadStart = 0.6230253f;

    public Mesh pointMesh, dirMesh, bombMesh;
    bool isSelected;

    private void Awake()
    {
        //editor = Camera.main.transform.GetComponent<SongEditor>();
        editor = Camera.main.transform.GetComponent<EditorScript>();
    }
    public void OnStart()
    {

    }

    public void Render()
    {
        if (beatCubeClass.type == BeatCubeClass.Type.Line)
        {
            transform.localScale = new Vector3(2, 2, 2);
            LineRenderer line = GetComponent<LineRenderer>();

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            line.positionCount = beatCubeClass.linePoints.ToArray().Length;
            //line.positionCount = beatCubeClass.x.Count;
            for (int i = 0; i < line.positionCount; i++)
            {
                float pointRoadPos = ((beatCubeClass.linePoints[i].x - (i != 0 ? road : 0)) * 2.5f - 1.25f) / 2f;

                line.SetPosition(i, new Vector3(pointRoadPos, beatCubeClass.linePoints[i].y, beatCubeClass.linePoints[i].z * 20));
                GameObject point = Instantiate(linePointPrefab, line.transform);
                point.transform.localPosition = line.GetPosition(i);
            }

            line = GetComponent<LineRenderer>();
            for (int i = 0; i < line.positionCount - 1; i++)
            {
                AddColliderToLine(line, line.GetPosition(i), line.GetPosition(i + 1));
            }
        }
        else if (beatCubeClass.type == BeatCubeClass.Type.Bomb)
        {
            Debug.Log("Bomb render");
            GetComponent<MeshFilter>().mesh = bombMesh;
        }
        else
        {
            GetComponent<MeshFilter>().mesh = beatCubeClass.type == BeatCubeClass.Type.Point ? pointMesh : dirMesh;

            //GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", beatCubeClass.saberType == 1 ? new Color32(255, 70, 0, 255) : beatCubeClass.saberType == -1 ? new Color32(0, 170, 255, 255) : new Color32(255, 255, 255, 255));
            //GetComponent<MeshRenderer>().materials[1].SetColor("_Color", beatCubeClass.saberType == 1 ? new Color32(255, 70, 0, 255) : beatCubeClass.saberType == -1 ? new Color32(0, 170, 255, 255) : new Color32(255, 255, 255, 255));
            if (isSelected) OnSelect();
            else OnDeselect();

            if (beatCubeClass.type == BeatCubeClass.Type.Dir)
            {
                transform.eulerAngles = new Vector3(0, 0, (int)beatCubeClass.subType * 45);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }

        // Legacy
        //transform.localPosition = new Vector3(unitsRoadStart + (beatCubeClass.road == -1 ? road : beatCubeClass.road), beatCubeClass.level == 0 ? 1 : 2, beatCubeClass.time * 20);
        if (beatCubeClass.road == -1)
        {
            road = Random.Range(0, 4);
        }
        else
        {
            road = beatCubeClass.road;
        }
        float roadPos = (road - 1) * 2.5f - 1.25f + 0.5f;
        float heightPos = beatCubeClass.level * 2.5f + 1.5f;
        transform.localPosition = new Vector3(roadPos, heightPos, beatCubeClass.time * 60);
    }

    bool tickPlayed;
    private void Update()
    {
        if (beatCubeClass.time - editor.aSource.time > 0)
        {
            tickPlayed = false;
        }
        else
        {
            if (!tickPlayed && editor.aSource.isPlaying)
            {
                tickPlayed = true;
                Color clr = new Color(191f / 255f, 72f / 255f, 0);
                editor.roads[road].material.SetColor("_EmissionColor", clr * 2f);
            }
        }
    }

    public void OnDeselect()
    {
        isSelected = false;
        if (beatCubeClass.type == BeatCubeClass.Type.Line)
        {
            GetComponent<LineRenderer>().materials[0].SetColor("_EmissionColor", Color.white * 1.1f);
            GetComponent<LineRenderer>().materials[0].SetColor("_Color", Color.white * 1.1f);
            foreach (Transform child in transform)
            {
                if (child.GetComponent<MeshRenderer>() == null) continue;
                child.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", new Color(0, 0.43f * 2, 0.75f * 2, 1));
                child.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", new Color(0, 0.43f * 2, 0.75f * 2, 1));
            }
        }
        else
        {
            GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", beatCubeClass.saberType == 1 ? new Color32(255, 70, 0, 255) : beatCubeClass.saberType == -1 ? new Color32(0, 170, 255, 255) : new Color32(255, 255, 255, 255));
            GetComponent<MeshRenderer>().materials[1].SetColor("_Color", beatCubeClass.saberType == 1 ? new Color32(255, 70, 0, 255) : beatCubeClass.saberType == -1 ? new Color32(0, 170, 255, 255) : new Color32(255, 255, 255, 255));
        }
    }
    public void OnSelect()
    {
        OnSelect(new Color32(20, 255, 0, 255));
    }
    public void OnSelect(Color32 color)
    {
        isSelected = true;

        if (beatCubeClass.type == BeatCubeClass.Type.Line)
        {
            GetComponent<LineRenderer>().materials[0].SetColor("_EmissionColor", color);
            GetComponent<LineRenderer>().materials[0].SetColor("_Color", color);
            foreach (Transform child in transform)
            {
                if (child.GetComponent<MeshRenderer>() == null) continue;
                child.GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", color);
                child.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", color);
            }
        }
        else
        {
            GetComponent<MeshRenderer>().materials[1].SetColor("_EmissionColor", color);
            GetComponent<MeshRenderer>().materials[1].SetColor("_Color", color);
        }
    }




    private void AddColliderToLine(LineRenderer line, Vector3 startPoint, Vector3 endPoint)
    {
        //create the collider for the line
        BoxCollider lineCollider = new GameObject("LineCollider").AddComponent<BoxCollider>();
        //set the collider as a child of your line
        lineCollider.transform.parent = line.transform;
        // get width of collider from line 
        float lineWidth = line.endWidth;
        // get the length of the line using the Distance method
        float lineLength = Vector3.Distance(startPoint, endPoint);
        // size of collider is set where X is length of line, Y is width of line
        //z will be how far the collider reaches to the sky
        //lineCollider.size = new Vector3(lineLength, lineWidth, lineWidth);
        lineCollider.size = new Vector3(lineLength * 3, lineWidth, lineWidth);
        // get the midPoint
        Vector3 midPoint = (startPoint + endPoint) / 2;
        // move the created collider to the midPoint
        lineCollider.transform.localPosition = midPoint;


        //heres the beef of the function, Mathf.Atan2 wants the slope, be careful however because it wants it in a weird form
        //it will divide for you so just plug in your (y2-y1),(x2,x1)
        float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));

        // angle now holds our answer but it's in radians, we want degrees
        // Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
        angle *= Mathf.Rad2Deg;

        //were interested in the inverse so multiply by -1
        angle *= -1;
        // now apply the rotation to the collider's transform, carful where you put the angle variable
        // in 3d space you don't wan't to rotate on your y axis
        lineCollider.transform.Rotate(0, angle, 0);
    }
}