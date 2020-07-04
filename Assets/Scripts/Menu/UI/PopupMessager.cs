using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModernEditor.Popup
{
    public class PopupMessager : MonoBehaviour
    {
        public static PopupMessager instance;

        [Header("UI")]
        public GameObject overlay;
        public Image image;
        public Text titleText;
        public Text messageText;

        public Sprite successTexture, errorTexture;

        private void OnEnable()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private static void Show(Sprite icon, string title, string message)
        {
            instance.overlay.SetActive(true);
            instance.titleText.text = title;
            instance.messageText.text = message;
            instance.image.sprite = icon;
        }
        public static void ShowError(string title, string message)
        {
            Show(instance.errorTexture, title, message);
        }
        public static void ShowSuccess(string title, string message)
        {
            Show(instance.successTexture, title, message);
        }

        public void Close()
        {
            instance.overlay.SetActive(false);
        }
    }
}