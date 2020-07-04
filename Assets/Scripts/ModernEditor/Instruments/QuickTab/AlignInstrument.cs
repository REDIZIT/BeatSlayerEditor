using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlignInstrument : MonoBehaviour
{
    public ModernEditorManager manager;

    [Header("UI")]
    public Button alignBtn;

    private void Update()
    {
        bool isSelected = manager.inspector.selectedCube != null;

        alignBtn.interactable = isSelected;
    }

    public void DoAlign()
    {
        manager.asource.time = manager.inspector.selectedCube.GetClass().time;
    }
}