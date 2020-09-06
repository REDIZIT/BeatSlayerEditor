using ModernEditor.Instruments;
using UnityEngine;
using UnityEngine.UI;

namespace InEditor.Inspector
{
    public class SaberHandTool : MonoBehaviour, ITool
    {
        [Header("UI")]
        public Image rightOutline;
        public Image leftOutline, bothOutline;
        public Image rightBg, leftBg, bothBg;
        public Image rightImage, leftImage, bothLeftImage, bothRightImage;

        public Color rightColor, leftColor;

        private BeatCubeClass cls;
        private InspectorTool tool;

        public void Refresh(BeatCubeClass cls, InspectorTool tool)
        {
            this.cls = cls;
            this.tool = tool;

            RefreshSelection();
        }

        public void OnSaberChange(int hand)
        {
            cls.saberType = hand;
            RefreshSelection();

            tool.OnToolChanged();
        }

        private void RefreshSelection()
        {
            if (cls.saberType == 0) EnableSelectionBoth();
            else if (cls.saberType == -1) EnableSelectionLeft();
            else if (cls.saberType == 1) EnableSelectionRight();
            else DisableSelection();
        }



        private void DisableSelection()
        {
            Colorize(rightOutline, rightColor, false);
            Colorize(leftOutline, leftColor, false);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, rightColor, false);
            Colorize(leftImage, leftColor, false);
            Colorize(bothLeftImage, leftColor, false);
            Colorize(bothRightImage, rightColor, false);

            ColorizeBackground(rightBg, rightColor, false);
            ColorizeBackground(leftBg, leftColor, false);
            ColorizeBackground(bothBg, Color.white, false);
        }
        private void EnableSelectionBoth()
        {
            Colorize(rightOutline, rightColor, false);
            Colorize(leftOutline, leftColor, false);
            Colorize(bothOutline, Color.white, true);

            Colorize(rightImage, rightColor, false);
            Colorize(leftImage, leftColor, false);
            Colorize(bothLeftImage, leftColor, true);
            Colorize(bothRightImage, rightColor, true);

            ColorizeBackground(rightBg, rightColor, false);
            ColorizeBackground(leftBg, leftColor, false);
            ColorizeBackground(bothBg, Color.white, true);
        }
        private void EnableSelectionRight()
        {
            Colorize(rightOutline, rightColor, true);
            Colorize(leftOutline, leftColor, false);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, rightColor, true);
            Colorize(leftImage, leftColor, false);
            Colorize(bothLeftImage, leftColor, false);
            Colorize(bothRightImage, rightColor, false);

            ColorizeBackground(rightBg, rightColor, true);
            ColorizeBackground(leftBg, leftColor, false);
            ColorizeBackground(bothBg, Color.white, false);
        }
        private void EnableSelectionLeft()
        {
            Colorize(rightOutline, rightColor, false);
            Colorize(leftOutline, leftColor, true);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, rightColor, false);
            Colorize(leftImage, leftColor, true);
            Colorize(bothLeftImage, leftColor, false);
            Colorize(bothRightImage, rightColor, false);

            ColorizeBackground(rightBg, rightColor, false);
            ColorizeBackground(leftBg, leftColor, true);
            ColorizeBackground(bothBg, Color.white, false);
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
