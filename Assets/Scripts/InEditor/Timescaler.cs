using InGame.Game.Spawn;
using UnityEngine;

namespace InEditor.BPM
{
    public class Timescaler : MonoBehaviour
    {
        [SerializeField] private EditorBeatManager bm;
        [SerializeField] private Transform timescaleTransform;

        public void Build(int beatsCount, float bpm)
        {
            foreach (Transform item in timescaleTransform) if (item.name != "Item") Destroy(item.gameObject);
            GameObject prefab = timescaleTransform.GetChild(0).gameObject;
            prefab.SetActive(true);

            for (int i = 0; i < beatsCount; i++)
            {
                GameObject item = Instantiate(prefab, timescaleTransform);

                item.transform.localPosition = new Vector3(0, 0, bm.fieldLength * i / beatsCount);

                BeatIndicator indicator = item.GetComponent<BeatIndicator>();
                indicator.Setup(bpm, i / (float)beatsCount, beatsCount, i == 0);
            }

            prefab.SetActive(false);
        }

        public void SetBPM(float bpm)
        {
            Build(16, bpm);
        }
    }
}
