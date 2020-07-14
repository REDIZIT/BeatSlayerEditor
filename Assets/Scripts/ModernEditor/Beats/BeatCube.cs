using InGame.Game.Spawn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class is for Editor. There is not a lot of content
/// </summary>
public class BeatCube : MonoBehaviour, IEditorBeat
{
    public BeatCubeClass cls;
    public BeatCubeClass GetClass() { return cls; }

    public MeshRenderer renderer;
    public MeshFilter filter;

    public Mesh pointMesh, arrowMesh;
    public GameObject cubeModel, bombModel;

    // Editor stuff
    EditorBeatManager bm;
    public Color leftArrowColor, rightArrowColor, anyArrowColor;
    public Color leftBorderColor, rightBorderColor, anyBorderColor;
    public Color selectedColor, unselectedColor;

    public bool isSelected;



    public void Setup(BeatCubeClass cls, EditorBeatManager bm)
    {
        this.cls = cls;
        this.bm = bm;

        Refresh();
    }

    public void Refresh()
    {

        cubeModel.SetActive(cls.type != BeatCubeClass.Type.Bomb);
        bombModel.SetActive(cls.type == BeatCubeClass.Type.Bomb);

        if (cls.type == BeatCubeClass.Type.Point)
        {
            filter.mesh = pointMesh;
        }
        else if (cls.type == BeatCubeClass.Type.Bomb)
        {
        }
        else
        {
            filter.mesh = arrowMesh;
            float angle = (int)cls.subType * 45;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
        
        Color saberColor = cls.saberType == 1 ? rightBorderColor : cls.saberType == -1 ? leftBorderColor : anyBorderColor;
        Color arrowColor = cls.saberType == 1 ? rightArrowColor : cls.saberType == -1 ? leftArrowColor : anyArrowColor;
        saberColor *= 2;
        arrowColor *= 2;
        
        renderer.materials[1].SetColor("_Color", isSelected ? selectedColor : saberColor);
        renderer.materials[1].SetColor("_EmissionColor", isSelected ? selectedColor :saberColor);
        renderer.materials[2].SetColor("_Color", arrowColor);
        renderer.materials[2].SetColor("_EmissionColor", arrowColor);
        

        float y = cls.level == 0 ? 0.8f : 4.6f;
        Vector3 pos = new Vector3(-3.5f + cls.road * 2.25f, y, bm.fieldLength * cls.time);
        transform.localPosition = pos;
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

    public void Delete()
    {
        Destroy(gameObject);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}

public interface IEditorBeat
{
    void Setup(BeatCubeClass cls, EditorBeatManager bm);
    void OnPoint();
    void OnDeselect();
    BeatCubeClass GetClass();
    void Delete();
    void Refresh();
    GameObject GetGameObject();
}