using UnityEngine;

namespace InEditor.Inspector
{
    public class LineEndPositionTool : RoadHeightTool
    {
        public override void Refresh()
        {
            RefreshToggles(cls.lineEndRoad, cls.lineEndLevel);
        }
        public override void ApplyChange(int road, int height)
        {
            cls.lineEndRoad = road;
            cls.lineEndLevel = height;
        }
    }
}
