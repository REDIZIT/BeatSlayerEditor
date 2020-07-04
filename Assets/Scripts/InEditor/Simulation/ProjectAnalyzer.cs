using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InEditor.Analyze
{
    public class ProjectAnalyzer : MonoBehaviour
    {
        private Simulator simulator;

        public ProjectAnalyzer()
        {
            simulator = new Simulator();
        }


        public List<AnalyzeResult> Analyze(Project proj)
        {

            List<Difficulty> diffs = GetDifficulties(proj);

            List<AnalyzeResult> results = new List<AnalyzeResult>();

            foreach (Difficulty diff in diffs)
            {
                int cubesCount = diff.beatCubeList.Where(c => c.type == BeatCubeClass.Type.Dir || c.type == BeatCubeClass.Type.Point).Count();
                int linesCount = diff.beatCubeList.Where(c => c.type == BeatCubeClass.Type.Line).Count();


                SimulationResult simResult = simulator.Simulate(diff);

                results.Add(new AnalyzeResult()
                {
                    DifficultyName = diff.name,
                    DifficultyStars = diff.stars,
                    DifficultyId = diff.id,
                    CubesCount = cubesCount,
                    LinesCount = linesCount,
                    MaxScore = simResult.MaxScore,
                    MaxRP = simResult.MaxRP,
                    ScorePerBlock = simResult.ScorePerBlock,
                    RPPerBlock = simResult.RPPerBlock
                });
            }

            return results;
        }

        private List<Difficulty> GetDifficulties(Project proj)
        {
            List<Difficulty> diffs = new List<Difficulty>();


            if(proj.difficulties.Count > 0)
            {
                diffs.AddRange(proj.difficulties);
            }
            else
            {
                diffs.Add(new Difficulty()
                {
                    beatCubeList = proj.beatCubeList,
                    name = proj.difficultName,
                    stars = proj.difficultStars == 0 ? 4 : proj.difficultStars,
                    id = -1
                });
            }

            return diffs;
        }
    }

    public class SimulationResult 
    {
        public float MaxScore { get; set; }
        public float MaxRP { get; set; }
        public float ScorePerBlock { get; set; }
        public float RPPerBlock { get; set; }
    }

    public class AnalyzeResult
    {
        public string DifficultyName { get; set; }
        public int DifficultyStars { get; set; }
        public int DifficultyId { get; set; }


        public float MaxScore { get; set; }
        public float MaxRP { get; set; }
        public int CubesCount { get; set; }
        public int LinesCount { get; set; }
        public float ScorePerBlock { get; set; }
        public float RPPerBlock { get; set; }
    }
}
