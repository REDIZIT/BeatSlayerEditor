using System.Collections;
using System.Collections.Generic;
using InGame.Game.Spawn;
using UnityEngine;

namespace ModernEditor.Difficulties
{
    public class DifficultyManager : MonoBehaviour
    {
        public ModernEditorManager manager;
        public EditorBeatManager bm;

        public Difficulty selectedDifficulty;
        
        public void LoadDifficulty(Difficulty difficulty)
        {
            selectedDifficulty = difficulty;
            
            bm.LoadBeats(selectedDifficulty.beatCubeList);

            manager.UpdateProjectTexts();
        }

        // Save button
        public void SaveDifficulty()
        {
            selectedDifficulty.beatCubeList = bm.beatLs;

            // Cut off cubes are out of music duration
            selectedDifficulty.beatCubeList.RemoveAll(c => c.time < 0 || c.time > bm.asource.clip.length);

            manager.SaveProject();
        }

        public void DeleteDifficulty(Difficulty difficulty)
        {
            int index = manager.project.difficulties.IndexOf(difficulty);
            manager.project.difficulties.RemoveAt(index);
            int loadIndex = index - 1;
            loadIndex = loadIndex < 0 ? 0 : loadIndex;
            
            LoadDifficulty(manager.project.difficulties[loadIndex]);
        }

        public void MoveDifficultyIndex(int current, int direction)
        {
            int fromIndex = current;
            int destIndex = current + direction;

            Difficulty tmp = manager.project.difficulties[fromIndex];
            manager.project.difficulties[fromIndex] = manager.project.difficulties[destIndex];
            manager.project.difficulties[destIndex] = tmp;
        }
    }
}