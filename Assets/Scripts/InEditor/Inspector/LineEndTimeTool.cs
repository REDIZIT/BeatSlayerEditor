using System;

namespace InEditor.Inspector
{
    public class LineEndTimeTool : TimeTool
    {
        public override void RefreshFields()
        {
            TimeSpan span = TimeSpan.FromSeconds(cls.lineLenght);
            minsField.text = span.Minutes.ToString();
            secsField.text = span.Seconds.ToString();
            msField.text = span.Milliseconds.ToString();
        }
        public override void ApplyTime(float seconds)
        {
            cls.lineLenght = seconds;
            inspector.OnToolChanged();
        }
    }
}
