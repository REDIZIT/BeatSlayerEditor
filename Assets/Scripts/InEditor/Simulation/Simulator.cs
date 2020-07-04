using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InEditor.Analyze
{
    public class Simulator
    {
        public SimulationResult result;

        // Values from game (ScoringManager.cs)

        private float comboValue = 0, comboValueMax = 16;
        private float comboMultiplier = 1;
        private float maxCombo = 1;
        /// <summary>
        /// Value from mods
        /// </summary>
        private float scoreMultiplier = 1;

        /// <summary>
        /// Value for smooth earning score on line slices
        /// </summary>
        private float earnedScore;




        public SimulationResult Simulate(Difficulty diff)
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            Reset();


            result = new SimulationResult();

            for (int i = 0; i < diff.beatCubeList.Count; i++)
            {
                // Spawned beat note (cube or line)
                BeatCubeClass cls = diff.beatCubeList[i];

                if(cls.type == BeatCubeClass.Type.Line)
                {
                    OnLineHold(cls);
                    OnLineHit();
                }
                else
                {
                    OnCubeHit();
                }

                ClampCombo();
            }

            
            float blocksCount = diff.beatCubeList.Count;

            result.MaxRP = GetRP(1, diff.stars, blocksCount, 0, diff.speed, 1);
            result.ScorePerBlock = result.MaxScore / blocksCount;
            result.RPPerBlock = result.MaxRP / blocksCount;

            Debug.Log($"Simulation time is {w.ElapsedMilliseconds}ms");
            return result;
        }

        private void Reset()
        {
            comboValue = 0;
            comboValueMax = 16;
            comboMultiplier = 1;
            maxCombo = 1;
            scoreMultiplier = 1;
            earnedScore = 0;
        }


        private void ClampCombo()
        {
            // Code from game (ScoringManager.cs)
            // == // == == Combo == == // == //

            if (comboValue >= comboValueMax && comboMultiplier < 16)
            {
                comboValue = 2;
                comboMultiplier *= 2;
                comboValueMax = 8 * comboMultiplier;
            }
            else if (comboValue <= 0)
            {
                if (comboMultiplier != 1)
                {
                    comboMultiplier /= 2;
                    comboValue = comboValueMax - 5;
                }
                else
                {
                    comboValue = 0;
                }
            }
            if (comboValue > 0)
            {
                //comboValue -= Time.deltaTime * comboMultiplier * 0.4f;
            }

            if (comboMultiplier > maxCombo) maxCombo = comboMultiplier;



            // == // == == Lines == == // == //
            // Seems not working xD

            if (earnedScore >= 1)
            {
                float rounded = Mathf.FloorToInt(earnedScore) * scoreMultiplier;
                earnedScore -= rounded;
                result.MaxScore += rounded;
            }
        }



        private void OnCubeHit()
        {
            result.MaxScore += comboMultiplier * scoreMultiplier;
            //result.CubesSliced++;
            comboValue += 1;
        }

        /// <summary>
        /// Invoked every frame while line is exists
        /// </summary>
        private void OnLineHold(BeatCubeClass cls)
        {
            float frameCount = cls.lineLenght * 60;
            result.MaxScore += comboMultiplier * scoreMultiplier * 0.04f * (frameCount);
        }

        private void OnLineHit()
        {
            //Replay.CubesSliced++;
            comboValue += 1;
        }



        private float GetRP(float accuracy, float difficulty, float cubesCount, int missed, float cubesSpeed, float musicSpeed)
        {
            float modsMultiplier = (cubesSpeed * 1 + musicSpeed * 1) / 2f;

            Debug.Log(accuracy + " " + difficulty + " " + cubesCount + " " + missed + " " + cubesSpeed + " " + musicSpeed);

            missed += 1;
            return (accuracy * difficulty * cubesCount * modsMultiplier) / missed;
        }
    }
}