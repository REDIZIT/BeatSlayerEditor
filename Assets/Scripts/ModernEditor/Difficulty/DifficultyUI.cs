using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InGame.Game.Spawn;
using UnityEngine;
using UnityEngine.UI;

namespace ModernEditor.Difficulties
{
    public class DifficultyUI : MonoBehaviour
    {
        public EditorBeatManager bm;
        public DifficultyManager dm;
        public ModernEditorManager manager;


        public GameObject window, editWindow, addWindow;
        public Transform itemContent;
        public Sprite starEmpty, starFilled;

        [Header("Edit")] 
        public Transform starContent;
        public InputField nameField, speedField;
        private DifficultyUIItem editingItem;
        
        [Header("Add")]
        public Transform addStarContent;
        public InputField addNameField, addSpeedField;
        public Dropdown addCopyFromDropdown;

        private const float MaxDifficultySpeed = 8;


        public void RefreshList()
        {
            foreach(Transform child in itemContent) if (child.name != "Item") Destroy(child.gameObject);
            GameObject prefab = itemContent.GetChild(0).gameObject;
            prefab.SetActive(true);

            for (int i = 0; i < manager.project.difficulties.Count; i++)
            {
                Difficulty difficulty = manager.project.difficulties[i];
                
                GameObject item = Instantiate(prefab, itemContent);
                item.GetComponent<DifficultyUIItem>().Setup(difficulty, i == 0, i == manager.project.difficulties.Count - 1, i);
            }

            float prefabHeight = prefab.GetComponent<RectTransform>().sizeDelta.y;
            prefab.SetActive(false);

            float height = prefabHeight * manager.project.difficulties.Count + 10 * (manager.project.difficulties.Count - 1);
            itemContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
        }
        public void OpenDifficultyInstrument()
        {
            window.SetActive(true);
            editWindow.SetActive(false);
            RefreshList();
        }

        
        
        
        public void Open(DifficultyUIItem item)
        {
            window.SetActive(false);
            editWindow.SetActive(false);
            dm.LoadDifficulty(item.difficulty);
        }
        public void Edit(DifficultyUIItem item)
        {
            editWindow.SetActive(true);
            editingItem = item;

            RefreshEdit();
        }

        

        public void Move(int current, int direction)
        {
            dm.MoveDifficultyIndex(current, direction);
            RefreshList();
        }
        

        #region Stars

        void RefreshStar(Transform content, int stars)
        {
            foreach(Transform child in content) if (child.name != "Star") Destroy(child.gameObject);
            GameObject prefab = content.GetChild(0).gameObject;
            prefab.SetActive(true);

            Color32 disabledColor = new Color32(45,45,45,255);
            Color32 enabledColor = new Color32(255,255,255,255);
            
            for (int i = 0; i < 10; i++)
            {
                GameObject star = Instantiate(prefab, content);
                bool filled = i < stars;

                star.name = i + "";
                Image img = star.transform.GetChild(0).GetComponent<Image>();
                img.sprite = filled ? starFilled : starEmpty;
                img.color = filled ? enabledColor : disabledColor;
            }
            
            prefab.SetActive(false);
        }

        int GetStars(Transform content)
        {
            int stars = 0;
            foreach (Transform child in content)
            {
                if(child.name == "Star") continue;
                
                Image img = child.GetChild(0).GetComponent<Image>();
                if (img.sprite == starFilled) stars++;
                else return stars;
            }
            
            return 0;
        }

        #endregion
        

        
        #region Editing

        public void RefreshEdit()
        {
            nameField.text = editingItem.difficulty.name;
            speedField.text = editingItem.difficulty.speed + "";

            RefreshStar(starContent, editingItem.difficulty.stars);
        }

        public void DeleteDifficulty()
        {
            dm.DeleteDifficulty(editingItem.difficulty);
            editWindow.SetActive(false);
            RefreshList();
        }
        
        
        public void OnNameChange()
        {
            editingItem.difficulty.name = nameField.text;
            
            RefreshList();
        }
        public void OnStarChange(Transform starItem)
        {
            int index = int.Parse(starItem.name);

            editingItem.difficulty.stars = index + 1;
            
            RefreshEdit();
            RefreshList();
        }
        public void OnSpeedChange()
        {
            try
            {
                speedField.text = speedField.text.Replace(",", ".");
                float speed = float.Parse(speedField.text, CultureInfo.InvariantCulture.NumberFormat);
                speed = Mathf.Clamp(speed, 0.1f, MaxDifficultySpeed);

                editingItem.difficulty.speed = speed;

                RefreshList();
            }
            catch (Exception err)
            {
            }
        }
        

        #endregion
        
        #region Adding new

        public void OpenAddWindow()
        {
            RefreshAddNew();
        }
        public void AddNewBtnClicked()
        {
            Difficulty d = ParseAddNewValues();
            if (d == null) return;
            if (manager.project.difficulties.Any(c => c.name == d.name)) return;

            d.id = manager.project.lastGivenDifficultyId + 1;
            manager.project.lastGivenDifficultyId += 1;
            
            manager.project.difficulties.Add(d);
            addWindow.SetActive(false);
            
            RefreshList();
        }
        
        
        
        private void RefreshAddNew()
        {
            addWindow.SetActive(true);

            addNameField.text = "";
            addSpeedField.text = "1";
            RefreshStar(addStarContent, 1);
            
            // Filling CopyFromDropdown
            addCopyFromDropdown.options = new List<Dropdown.OptionData>();
            addCopyFromDropdown.options.Add(new Dropdown.OptionData("[Don't copy]"));
            
            foreach (var diffName in manager.project.difficulties.Select(c => c.name))
            {
                addCopyFromDropdown.options.Add(new Dropdown.OptionData(diffName));
            }
            addCopyFromDropdown.value = 0;
            addCopyFromDropdown.RefreshShownValue();
        }
        
        Difficulty ParseAddNewValues()
        {
            if (addSpeedField.text == "") return null;
            float speed = float.Parse(addSpeedField.text, NumberStyles.AllowDecimalPoint);
            speed = Mathf.Clamp(speed, 0.1f, MaxDifficultySpeed);
            
            
            List<BeatCubeClass> ls = new List<BeatCubeClass>();
            if (addCopyFromDropdown.value != 0)
            {
                Difficulty copyFromDiff = manager.project.difficulties[addCopyFromDropdown.value - 1];
                ls.AddRange(copyFromDiff.beatCubeList);
            }
            
            
            Difficulty d = new Difficulty()
            {
                name = addNameField.text,
                stars = GetStars(addStarContent),
                speed = speed,
                beatCubeList = ls
            };

            return d;
        }

        public void OnAddStarChange(Transform starItem)
        {
            int index = int.Parse(starItem.name);

            RefreshStar(addStarContent, index + 1);
            RefreshList();
        }
        
        #endregion
    }
}