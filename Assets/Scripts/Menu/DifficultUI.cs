using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyEditor
{
    public class DifficultUI : MonoBehaviour
    {
        public ProjectUI projectUI;

        public GameObject locker;
        public InputField nameField;

        public Transform starsContent;
        public Sprite fullStar, emptyStar;

        public void Show()
        {
            locker.SetActive(true);

            //nameField.text = projectUI.selectedProject.difficultName;
            nameField.text = "Deprecated";

            ShowStars(4);
        }

        void ShowStars(int difficulty)
        {
            foreach (Transform child in starsContent) if(child.name != "StarBtn") Destroy(child.gameObject);
            GameObject prefab = starsContent.GetChild(0).gameObject;
            prefab.SetActive(true);

            for (int i = 1; i <= 10; i++)
            {
                Transform go = Instantiate(prefab, starsContent).transform;

                go.GetChild(0).GetComponent<Image>().sprite = i <= difficulty ? fullStar : emptyStar;
                go.GetChild(0).GetComponent<Image>().color = i <= difficulty ? Color.white : new Color(0.1750833f, 0.1750833f, 0.1750833f);

                go.name = i.ToString();
            }

            prefab.SetActive(false);
        }

        public void OnStarClicked(Transform btn)
        {
            int id = int.Parse(btn.name);

            //projectUI.selectedProject.difficultStars = id;

            ShowStars(id);
        }

        public void OnNameFieldChange()
        {
            //projectUI.selectedProject.difficultName = nameField.text;
        }
    }

}
