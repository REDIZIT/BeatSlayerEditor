using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePointItenScript : MonoBehaviour
{
    public void OnClick()
    {
        Camera.main.GetComponent<SongEditor>().ModifyLineEditPoint(gameObject);
    }
}
