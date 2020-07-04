using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatCubeGo_v2 : MonoBehaviour
{
    public BeatCubeClass bs;
    public Mesh arrowMesh;

    private void Awake()
    {
        Render();
    }
    void Render()
    {
        if(bs.type == BeatCubeClass.Type.Dir)
        {
            GetComponent<MeshFilter>().mesh = arrowMesh;
            int random = Random.Range(0, 3);
            float angle = bs.subType == BeatCubeClass.SubType.Down ? 0 :
                bs.subType == BeatCubeClass.SubType.Right ? 90 :
                bs.subType == BeatCubeClass.SubType.Up ? 180 :
                bs.subType == BeatCubeClass.SubType.Left ? -90 :
                random * 90;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

    void OnSelect()
    {

    }
}