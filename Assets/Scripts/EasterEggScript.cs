using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EasterEggScript : MonoBehaviour
{
    // Ну что, как ломается код?
    // Да-да, говнокод, можешь не говорить ^_^

    public RectTransform egg1Go;
    public Text egg1Text;


    int egg1Index = -1;
    public string[] egg1Phrases = new string[10]
    {
        "No you can't", "No!", "No, u can't do this!", "Don't click me!", "I said NO!", "I'll call cops!", "Ookay bro, I'm calling", "...", "Haha, u're trying to do that yet?!", "AAAAAA, FUCK OFF!!1!"
    };

    public void OnEgg1Clicked()
    {
        egg1Index++;
        if (egg1Index >= egg1Phrases.Length)
        {
            Application.Quit();
            return;
        }

        egg1Go.gameObject.SetActive(true);

        egg1Text.text = egg1Phrases[egg1Index];

        egg1Go.sizeDelta = new Vector2(egg1Text.preferredWidth + 35 * 2, egg1Go.sizeDelta.y);
    }
}