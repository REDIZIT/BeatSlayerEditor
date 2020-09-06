using ModernEditor.Instruments;
using UnityEngine;
using UnityEngine.UI;

namespace InEditor.Inspector
{
    public class SpawnTypeTool : MonoBehaviour, ITool
    {
        [Header("UI")]
        public Image rightOutline;
        public Image leftOutline;
        public Image rightBg, leftBg;
        public Image rightImage, leftImage, bombImage;

        public Color rightColor, leftColor;

        private BeatCubeClass cls;
        private InspectorTool tool;

        public void Refresh(BeatCubeClass cls, InspectorTool tool)
        {
            this.cls = cls;
            this.tool = tool;

            RefreshSelection();
        }

        public void OnTypeChange(int type)
        {
            cls.type = type == 0 ? BeatCubeClass.Type.Dir : BeatCubeClass.Type.Line;
            
            RefreshSelection();

            tool.OnToolChanged();
            tool.OnOpen(true);
        }

        private void RefreshSelection()
        {
            if (cls.type == BeatCubeClass.Type.Dir || cls.type == BeatCubeClass.Type.Point || cls.type == BeatCubeClass.Type.Bomb) 
                EnableSelectionLeft();
            else if (cls.type == BeatCubeClass.Type.Line) 
                EnableSelectionRight();
            else
                DisableSelection();
        }



        private void DisableSelection()
        {
            Colorize(rightOutline, rightColor, false);
            Colorize(leftOutline, leftColor, false);

            Colorize(rightImage, rightColor, false);
            Colorize(leftImage, leftColor, false);
            Colorize(bombImage, leftColor, false);

            ColorizeBackground(rightBg, rightColor, false);
            ColorizeBackground(leftBg, leftColor, false);
        }
        private void EnableSelectionRight()
        {
            Colorize(rightOutline, rightColor, true);
            Colorize(leftOutline, leftColor, false);

            Colorize(rightImage, rightColor, true);
            Colorize(leftImage, leftColor, false);
            Colorize(bombImage, leftColor, false);

            ColorizeBackground(rightBg, rightColor, true);
            ColorizeBackground(leftBg, leftColor, false);
        }
        private void EnableSelectionLeft()
        {
            Colorize(rightOutline, rightColor, false);
            Colorize(leftOutline, leftColor, true);

            Colorize(rightImage, rightColor, false);
            Colorize(leftImage, leftColor, true);
            Colorize(bombImage, leftColor, false);

            ColorizeBackground(rightBg, rightColor, false);
            ColorizeBackground(leftBg, leftColor, true);
        }


        private void Colorize(Image image, Color color, bool enabled)
        {
            image.color = new Color(color.r, color.g, color.b, color.a * (enabled ? 1 : 0.2f));
        }
        private void ColorizeBackground(Image image, Color color, bool enabled)
        {
            image.color = new Color(color.r, color.g, color.b, color.a * (enabled ? 0.2f : 0.02f));
        }
    }
}
