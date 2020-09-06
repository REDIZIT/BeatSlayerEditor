using ModernEditor.Instruments;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InEditor.Inspector
{
    public class RoadHeightTool : MonoBehaviour, ITool
    {
        [SerializeField] private ToggleGroup toggleGroup;

        protected BeatCubeClass cls;
        private InspectorTool tool;


        public void Refresh(BeatCubeClass cls, InspectorTool tool)
        {
            this.cls = cls;
            this.tool = tool;

            Refresh();
        }

        public void OnToggleChange()
        {
            Toggle activeToggle = GetActiveToggle(toggleGroup);

            int index = int.Parse(activeToggle.name);
            int height = index >= 4 ? 1 : 0;
            int road = index >= 4 ? index - 4 : index;

            ApplyChange(road, height);

            tool.OnToolChanged();
        }

        public virtual void Refresh()
        {
            RefreshToggles(cls.road, cls.level);
        }

        public virtual void ApplyChange(int road, int height)
        {
            cls.road = road;
            cls.level = height;
        }


        protected void RefreshToggles(int road, int height)
        {
            int targetIndex = road % 4 + height * 4;

            Toggle toggle = toggleGroup.transform.GetComponentsInChildren<Toggle>().FirstOrDefault(c => c.name == targetIndex.ToString());
            toggle.SetIsOnWithoutNotify(true);
        }

        private Toggle GetActiveToggle(ToggleGroup group)
        {
            foreach (Toggle toggle in toggleGroup.transform.GetComponentsInChildren<Toggle>())
            {
                if (toggle.isOn) return toggle;
            }

            return null;
        }
    }
}
