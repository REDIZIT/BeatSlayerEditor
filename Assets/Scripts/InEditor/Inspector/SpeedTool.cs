using ModernEditor.Instruments;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace InEditor.Inspector
{
    public class SpeedTool : MonoBehaviour, ITool
    {
        [SerializeField] private InputField speedField;

        private BeatCubeClass cls;
        private InspectorTool tool;

        public void Refresh(BeatCubeClass cls, InspectorTool tool)
        {
            this.cls = cls;
            this.tool = tool;

            speedField.text = cls.speed.ToString();
        }

        public void OnInputFieldChanged()
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            string speedStr = speedField.text.Replace(",", ".");
            cls.speed = float.Parse(speedStr, NumberStyles.Any, ci);

            tool.OnToolChanged();
        }
    }
}
