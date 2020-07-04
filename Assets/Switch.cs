using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Switch : MonoBehaviour, IPointerDownHandler
{
    Animator animator { get { return GetComponent<Animator>(); } }

    public Action<Switch> onChangeCallback;


    public Color color_on, color_off;

    public Image background, knob;

    bool _isOn;
    public bool isOn
    {
        get { return _isOn; }
        set { if (value != _isOn) DoSwitch(); }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DoSwitch();
    }

    public void DoSwitch()
    {
        if (_isOn)
        {
            animator.Play("Switch_off");
            knob.color = color_off;
            background.color = color_off * 0.6f;
        }
        else
        {
            animator.Play("Switch_on");
            knob.color = color_on;
            background.color = color_on * 0.6f;
        }

        _isOn = !_isOn;

        onChangeCallback(this);
    }

    /// <summary>
    /// Set isOn without animation
    /// </summary>
    /// <param name="isOn"></param>
    public void Set(bool isOn)
    {
        //animator.Play(isOn ? "Switch_on" : "Switch_off", 0, 1);
        
        if (isOn)
        {
            knob.GetComponent<RectTransform>().anchoredPosition = new Vector2(29.1f, 0);
            knob.color = color_on;
            background.color = color_on * 0.6f;
        }
        else
        {
            knob.GetComponent<RectTransform>().anchoredPosition = new Vector2(-29.1f, 0);
            knob.color = color_off;
            background.color = color_off * 0.6f;
        }

        _isOn = isOn;
    }
}
