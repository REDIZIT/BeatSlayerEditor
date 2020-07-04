using InGame.Game.Spawn;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
/// This class is for Editor. There is not a lot of content
/// </summary>
public class BeatLine : MonoBehaviour, IEditorBeat
{
    public BeatCubeClass cls;

    EditorBeatManager bm;

    public Transform firstCap, secondCap;
    public CapsuleCollider lineCollider;
    public Transform cylinder;
    
    public float secondCapRoadPos;
    
    public bool isSelected;
    public Color selectedColor, defaultColor;
    
    

    public void Setup(BeatCubeClass cls, EditorBeatManager bm)
    {
        this.cls = cls;
        this.bm = bm;

        Refresh();
    }

    void Update()
    {
        CapMovement();
    }
    
    void CapMovement()
    {
        return;
        /*// Cap speed in units/frame
        float capSpeed = bm.CubeSpeed;
        float capMax = cls.linePoints[1].z * bm.fieldLength;

        // Offset to selected road second cap at spawn
        float capRoadOffsetTime = capMax / capSpeed;
        float capRoadOffsetDistance = secondCapRoadPos - transform.position.x;
        float capRoadOffsetSpeed = capRoadOffsetDistance / capRoadOffsetTime;

        
        secondCap.position += new Vector3(capRoadOffsetSpeed, 0, capSpeed);
        if(secondCap.localPosition.z > capMax)
        {
            secondCap.localPosition = new Vector3(0, secondCap.localPosition.y, capMax);
            secondCap.position = new Vector3(secondCapRoadPos, secondCap.position.y, secondCap.position.z);
        }
*/
        
        float y = cls.level == 0 ? 0.8f : 4.6f;
        Vector3 pos = new Vector3(-3.5f + cls.road * 2.25f, y, 100);
        transform.position = pos;

        secondCapRoadPos = -3.5f + cls.lineEndRoad * 2.25f;
        
        secondCap.localPosition = new Vector3(0, secondCap.localPosition.y, cls.linePoints[1].z * bm.fieldLength);
        

        float colliderZ = (firstCap.position.z + secondCap.position.z) / 2f;
        lineCollider.transform.position = new Vector3(lineCollider.transform.position.x, lineCollider.transform.position.y, colliderZ);

        float capsDistance = secondCap.position.z - firstCap.position.z;
        lineCollider.height = capsDistance + lineCollider.radius * 2;




        // Update cylinder between caps
        UpdateCylinder();
    }
    void UpdateCylinder()
    {
        float averageZ = (firstCap.position.z + secondCap.position.z) / 2f;
        float averageY = (firstCap.position.y + secondCap.position.y) / 2f;
        float averageX = (firstCap.position.x + secondCap.position.x) / 2f;

        float distance = Vector3.Distance(firstCap.position, secondCap.position);

        
        Vector3 cylinderPos = new Vector3(averageX, averageY, averageZ);
                
        cylinder.position = cylinderPos;
        cylinder.localScale = new Vector3(cylinder.localScale.x, distance / 2f, cylinder.localScale.z);

        cylinder.LookAt(secondCap.position);
        cylinder.localEulerAngles += new Vector3(90,0,0);

        
        lineCollider.transform.localEulerAngles = cylinder.localEulerAngles;
        lineCollider.transform.position = cylinder.position;
        
        
        float colliderZ = (firstCap.position.z + secondCap.position.z) / 2f;
        lineCollider.transform.position = new Vector3(lineCollider.transform.position.x, lineCollider.transform.position.y, colliderZ);

        float capsDistance = secondCap.position.z - firstCap.position.z;
        lineCollider.height = capsDistance + lineCollider.radius * 2;
    }
    public float GetRandom()
    {
        float value = Random.Range(-1, 1);
        if (value > 0) value += 0.5f;
        else if (value < 0) value -= 0.5f;
        else value = GetRandom();

        return value;
    }
    
    public void Refresh()
    {
        float y = cls.level == 0 ? 0.8f : 4.6f;
        Vector3 pos = new Vector3(-3.5f + cls.road * 2.25f, y, bm.fieldLength * cls.time);
        transform.localPosition = pos;

        float xPos = cls.linePoints.Count > 0 ? cls.linePoints[1].x : cls.lineEndRoad;
        float zPos = cls.linePoints.Count > 0 ? cls.linePoints[1].z * bm.fieldLength : cls.lineLenght * bm.fieldLength;
        float endLevel = cls.linePoints.Count > 0 ? cls.level : cls.lineEndLevel;
        float yPos = endLevel == 0 ? 0.8f : 4.6f;
        secondCapRoadPos = -3.5f + xPos * 2.25f;
        
        secondCap.localPosition = new Vector3(0, 0, zPos);
        secondCap.position = new Vector3(secondCapRoadPos, yPos, secondCap.position.z);

        cylinder.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", isSelected ? selectedColor : defaultColor);
        
        UpdateCylinder();
    }
    
    public void OnPoint()
    {
        bm.OnPoint(this);
        isSelected = true;
        Refresh();
    }

    public void OnDeselect()
    {
        isSelected = false;
        Refresh();
    }

    public BeatCubeClass GetClass() { return cls; }
    public void Delete()
    {
        Destroy(gameObject);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
