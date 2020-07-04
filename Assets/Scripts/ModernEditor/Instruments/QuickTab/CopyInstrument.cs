using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CopyInstrument : MonoBehaviour
{
    public ModernEditorManager manager;

    public List<IEditorBeat> selectedCubes;
    public List<IEditorBeat> copyingCubes, copiedCubes;
    public bool isSelecting;
    bool isCopying, isPlacing;
    float copiedTime;

    [Header("UI")]
    public Button copyBtn;
    public GameObject doCopyBtn, cancelBtn, pasteBtn;

    private void Update()
    {
        bool isSelected = manager.inspector.selectedCube != null;

        copyBtn.interactable = isSelected;

        CopyOperationUpdate();
    }

    
    
    public void CopyStart()
    {
        isCopying = true;
        isSelecting = true;
        copyingCubes = new List<IEditorBeat>();
        copiedCubes = new List<IEditorBeat>();
        
        cancelBtn.SetActive(true);
        doCopyBtn.SetActive(true);
        pasteBtn.SetActive(false);
        
        if(manager.inspector.selectedCube != null) copyingCubes.Add(manager.inspector.selectedCube);
    }
    public void CopyApply()
    {
        for (int i = 0; i < copyingCubes.Count; i++)
        {
            GameObject cube = Instantiate(copyingCubes[i].GetGameObject(), copyingCubes[i].GetGameObject().transform.parent);
            copyingCubes[i].OnDeselect();

            IEditorBeat beat = cube.GetComponent<IEditorBeat>();
            beat.Setup(beat.GetClass(), manager.bm);
            copiedCubes.Add(cube.GetComponent<IEditorBeat>());
        }
        isPlacing = true;
        copiedTime = manager.asource.time;
        
        doCopyBtn.SetActive(false);
        cancelBtn.SetActive(false);
        pasteBtn.SetActive(true);
    }
    public void CopyPlace()
    {
        isPlacing = false;
        isCopying = false;
        isSelecting = false;

        for (int i = 0; i < copiedCubes.Count; i++)
        {
            copiedCubes[i].OnDeselect();
        }
        manager.bm.beatLs.AddRange(copiedCubes.Select(c => c.GetClass()));

        copiedCubes.Clear();
        copyingCubes.Clear();
        
        cancelBtn.SetActive(false);
        doCopyBtn.SetActive(false);
        pasteBtn.SetActive(false);
    }
    public void CopyOperationUpdate()
    {
        if (!isCopying) return;

        if (isPlacing)
        {
            for (int i = 0; i < copiedCubes.Count; i++)
            {
                float cubeTime = copyingCubes[i].GetClass().time + (manager.asource.time - copiedTime);
                copiedCubes[i].GetClass().time = cubeTime;
                copiedCubes[i].Refresh();
            }
        }
    }

    public void OnCancelBtnClicked()
    {
        cancelBtn.SetActive(false);
        doCopyBtn.SetActive(false);
        pasteBtn.SetActive(false);
        
        copiedCubes.Clear();
        copyingCubes.Clear();
    }

    public void OnCubePoint(IEditorBeat beat)
    {
        copyingCubes.Add(beat);
    }

    public void OnCubeDeselect(IEditorBeat beat)
    {
        copyingCubes.Remove(beat);
    }
}
