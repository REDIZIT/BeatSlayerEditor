using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TimelineSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public Timeline timeline;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Application.isEditor) return;
        timeline.Pause();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Application.isEditor) return;
        //if (!eventData.dragging) timeline.Resume();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Application.isEditor) return;
        //timeline.Resume();
    }
}
