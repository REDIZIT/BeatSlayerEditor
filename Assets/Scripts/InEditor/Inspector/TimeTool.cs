using ModernEditor.Instruments;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace InEditor.Inspector
{
    public class TimeTool : MonoBehaviour, ITool
    {
        [Header("Components")]
        [SerializeField] private AudioSource asource;
        protected InspectorTool inspector;

        [Header("UI")]
        [SerializeField] protected InputField minsField;
        [SerializeField] protected InputField secsField, msField;

        [Header("Rewind")]
        [SerializeField] private float rewindSeconds = 0.05f;
        

        protected BeatCubeClass cls;

        public void Refresh(BeatCubeClass cls, InspectorTool inspector)
        {
            this.cls = cls;
            this.inspector = inspector;

            RefreshFields();
        }

        public void OnInputFieldChanged()
        {
            int minutes = int.Parse(minsField.text);
            int seconds = int.Parse(secsField.text);
            int milliseconds = int.Parse(msField.text);

            TimeSpan span = new TimeSpan(0, 0, minutes, seconds, milliseconds);

            ApplyTime((float)span.TotalSeconds);

            RefreshCube();
        }

        public virtual void RefreshFields()
        {
            TimeSpan span = TimeSpan.FromSeconds(cls.time);
            minsField.text = span.Minutes.ToString();
            secsField.text = span.Seconds.ToString();
            msField.text = span.Milliseconds.ToString();
        }
        public virtual void ApplyTime(float seconds)
        {
            cls.time = seconds;
        }





        public void RewindForward()
        {
            cls.time += rewindSeconds;
            cls.time = Mathf.Clamp(cls.time, 0, asource.clip.length);
            RefreshCube();
            RefreshFields();
        }
        public void RewindBackward()
        {
            cls.time -= rewindSeconds;
            cls.time = Mathf.Clamp(cls.time, 0, asource.clip.length);
            RefreshCube();
            RefreshFields();
        }



        private void RefreshCube()
        {
            inspector.selectedCube?.Refresh();
        }
    }
}
