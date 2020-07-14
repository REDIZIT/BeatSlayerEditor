using UnityEngine;

namespace InEditor.BPM
{
    public class BeatIndicator : MonoBehaviour
    {
        public MeshRenderer rend;
        public AudioSource asource;

        public float BPM { get; set; }
        public float Offset { get; set; }
        public int IndicatorsCount { get; set; }
        public bool IsFirst { get; set; }


        private float beatRangeTime;
        private float beatsPassedPrev;

        public void Setup(float bpm, float offset, int indicatorsCount, bool isFirst)
        {
            BPM = bpm;
            Offset = offset;
            IndicatorsCount = indicatorsCount;

            IsFirst = isFirst;
            rend.transform.localScale = IsFirst ? 
                new Vector3(rend.transform.localScale.x, rend.transform.localScale.y, 2.5f) :
                rend.transform.localScale;
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (!asource.isPlaying && IsFirst) return;

            if(BPM <= 0)
            {
                rend.material.SetColor("_EmissionColor", ClampColor(rend.material.GetColor("_EmissionColor") * 0.9f, 0.02f, 0.02f));
                return;
            }


            // Time in seconds which one beat take
            beatRangeTime = 1f / (BPM / 60f);

            if (IsFirst)
            {
                rend.material.SetColor("_EmissionColor", ClampColor(rend.material.GetColor("_EmissionColor") * 0.75f, 0.02f, 1));

                int beatsPassed = Mathf.CeilToInt((asource.time + Offset) / beatRangeTime);
                if (beatsPassedPrev != beatsPassed)
                {
                    if (beatsPassed > beatsPassedPrev)
                    {
                        rend.material.SetColor("_EmissionColor", new Color(1, .5f, 0, 1));
                    }
                    beatsPassedPrev = beatsPassed;
                }
            }
            else
            {
                // Make an offset for indicator wave effect
                float offsetedTime = asource.time + Offset;

                float clr = offsetedTime % beatRangeTime;

                //clr = Mathf.RoundToInt(clr * IndicatorsCount) / (float)IndicatorsCount;

                // Normalize color based on bpm
                clr *= BPM / 100f;

                clr *= clr;

                rend.material.SetColor("_EmissionColor", new Color(clr, clr, clr, 1));
            }
        }

        private Color ClampColor(Color clr, float min, float max)
        {
            float r = Mathf.Clamp(clr.r, min, max);
            float g = Mathf.Clamp(clr.g, min, max);
            float b = Mathf.Clamp(clr.b, min, max);
            float a = Mathf.Clamp(clr.a, min, max);
            return new Color(r, g, b, a);
        }
    }
}
