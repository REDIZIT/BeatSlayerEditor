using ModernEditor.Instruments;

namespace InEditor.Inspector
{
    public interface ITool
    {
        void Refresh(BeatCubeClass cls, InspectorTool tool);
    }
}
