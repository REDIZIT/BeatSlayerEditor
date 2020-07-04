using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnStateAnimator : MonoBehaviour
{
    public float targetWidth;
    public float delay = 3;
    public RectTransform rect;
    public bool isPlaying;


    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(Mathf.RoundToInt(rect.sizeDelta.x * 10) / 10f != Mathf.RoundToInt(targetWidth * 10) / 10f)
        {
            rect.sizeDelta += new Vector2((targetWidth - rect.sizeDelta.x) / 10f, 0);
        }
        else
        {
            if (isPlaying)
            {
                targetWidth = 0;
                isPlaying = false;
            }
        }
    }

    public void Play()
    {
        targetWidth = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
        isPlaying = true;
    }
}