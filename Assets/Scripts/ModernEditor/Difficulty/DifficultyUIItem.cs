using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModernEditor.Difficulties
{
    public class DifficultyUIItem : MonoBehaviour
    {
        public DifficultyUI ui;
        public Difficulty difficulty;
        private int current;
        
        public Text nameText, statText;
        public Text starText;

        public GameObject moveUpBtn, moveDownBtn;

        public void Setup(Difficulty difficulty, bool isFirst, bool IsLast, int current)
        {
            this.difficulty = difficulty;
            this.current = current;
            
            nameText.text = difficulty.name;

            int beatCount = difficulty.beatCubeList.Count;
            int linesCount = difficulty.beatCubeList.Where(c => c.type == BeatCubeClass.Type.Line).Count();
            statText.text = $"Beat count: {beatCount} (cubes: {beatCount - linesCount}, lines: {linesCount})\n" +
                            $"Speed: x{difficulty.speed}";
            
            starText.text = new string('●', difficulty.stars) + new string('○', 10 - difficulty.stars);
            
            moveUpBtn.SetActive(!isFirst);
            moveDownBtn.SetActive(!IsLast);
        }

        public void Edit()
        {
            ui.Edit(this);
        }

        public void Open()
        {
            ui.Open(this);
        }


        void Move(int direction)
        {
            ui.Move(current, direction);
        }
        public void MoveUp()
        {
            Move(-1);
        }

        public void MoveDown()
        {
            Move(1);
        }
    }
}