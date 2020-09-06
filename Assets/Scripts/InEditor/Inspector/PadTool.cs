using System;
using UnityEngine;
using UnityEngine.UI;
using ModernEditor.Instruments;
using System.Collections.Generic;

namespace InEditor.Inspector
{
    public class PadTool : MonoBehaviour, ITool
    {
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private Toggle bombToggle;

        public BeatCubeClass cls;
        private InspectorTool inspector;

        public void Refresh(BeatCubeClass cls, InspectorTool inspector)
        {
            this.cls = cls;
            this.inspector = inspector;

            List<Toggle> toggles = new List<Toggle>();
            toggles.Add(bombToggle);
            toggles.AddRange(toggleGroup.transform.GetComponentsInChildren<Toggle>());

            foreach (Toggle toggle in toggles)
            {
                if (cls.type == BeatCubeClass.Type.Bomb)
                {
                    bombToggle.SetIsOnWithoutNotify(true);
                    break;
                }
                toggle.SetIsOnWithoutNotify(toggle.name == cls.subType.ToString());
            }
        }


        public void OnToggleChange()
        {
            Toggle activeToggle = GetActiveToggle(toggleGroup);

            if (activeToggle.name == "Point")
            {
                cls.type = BeatCubeClass.Type.Point;
            }
            else if (activeToggle.name == "Bomb")
            {
                cls.type = BeatCubeClass.Type.Bomb;
            }
            else
            {
                cls.type = BeatCubeClass.Type.Dir;
                cls.subType = (BeatCubeClass.SubType)Enum.Parse(typeof(BeatCubeClass.SubType), activeToggle.name);
            }

            inspector.selectedCube?.Refresh();
        }

        private Toggle GetActiveToggle(ToggleGroup group)
        {
            List<Toggle> toggles = new List<Toggle>();
            toggles.Add(bombToggle);
            toggles.AddRange(toggleGroup.transform.GetComponentsInChildren<Toggle>());

            foreach (Toggle toggle in toggles)
            {
                if (toggle.isOn) return toggle;
            }

            return null;
        }
    }
}
