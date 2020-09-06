using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagnetInstrument : MonoBehaviour
{
    public ModernEditorManager manager;

    public List<IEditorBeat> selectedCubes;
    public bool isSelecting;

    [Header("UI")]
    public Button magnetBtn;
    public GameObject applyBtn, cancelBtn;

    private void Update()
    {
        bool isSelected = manager.inspector.selectedCube != null;

        magnetBtn.interactable = isSelected;
    }

    public void MagnetBtnClicked()
    {
        isSelecting = true;
        selectedCubes = new List<IEditorBeat>();
        selectedCubes.Add(manager.inspector.selectedCube);
        
        applyBtn.SetActive(true);
        cancelBtn.SetActive(true);
    }

    public void ApplyBtnClicked()
    {
        isSelecting = false;
        for (int i = 1; i < selectedCubes.Count; i++)
        {
            selectedCubes[i].GetClass().time = selectedCubes[0].GetClass().time;
            selectedCubes[i].OnDeselect();
            selectedCubes[i].Refresh();
        }
        selectedCubes.Clear();
        
        applyBtn.SetActive(false);
        cancelBtn.SetActive(false);
    }

    public void CancelBtnClicked()
    {
        for (int i = 1; i < selectedCubes.Count; i++)
        {
            selectedCubes[i].OnDeselect();
        }
        selectedCubes.Clear();
        isSelecting = false;
        
        applyBtn.SetActive(false);
        cancelBtn.SetActive(false);
    }


    public void OnCubePoint(IEditorBeat beat)
    {
        selectedCubes.Add(beat);
    }

    public void OnCubeDeselect(IEditorBeat beat)
    {
        selectedCubes.Remove(beat);
    }
}
