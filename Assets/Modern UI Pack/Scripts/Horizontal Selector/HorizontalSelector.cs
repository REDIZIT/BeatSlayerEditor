﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class HorizontalSelector : MonoBehaviour
    {
        private TextMeshProUGUI label;
        private TextMeshProUGUI labeHelper;
        private Animator selectorAnimator;

        [Header("SETTINGS")]
        public string selectorTag = "Tag Text";
        public int defaultIndex = 0;
        public bool saveValue;
        public bool invokeAtStart;
        public bool invertAnimation;
        public bool loopSelection;
        public int index = 0;

        [Header("ITEMS")]
        public List<Item> itemList = new List<Item>();

        [System.Serializable]
        public class Item
        {
            public string itemTitle = "Item Title";
            public UnityEvent onValueChanged;
        }

        void Awake()
        {
            selectorAnimator = gameObject.GetComponent<Animator>();
            label = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            labeHelper = transform.Find("Text Helper").GetComponent<TextMeshProUGUI>();

            if (saveValue == true)
                defaultIndex = PlayerPrefs.GetInt(selectorTag + "HSelectorValue");

            if (invokeAtStart == true)
                itemList[index].onValueChanged.Invoke();

            index = defaultIndex;
            RefreshTitle();
        }
        public void RefreshTitle()
        {
            if (itemList.Count == 0) return;

            label.text = itemList[Mathf.Clamp(index, 0, itemList.Count - 1)].itemTitle;
            labeHelper.text = label.text;
        }

        public void PreviousClick()
        {
            if (loopSelection == false)
            {
                if (index != 0)
                {
                    labeHelper.text = label.text;

                    if (index == 0)
                        index = itemList.Count - 1;

                    else
                        index--;

                    label.text = itemList[index].itemTitle;
                    itemList[index].onValueChanged.Invoke();

                    selectorAnimator.Play(null);
                    selectorAnimator.StopPlayback();

                    if (invertAnimation == true)
                        selectorAnimator.Play("Forward");
                    else
                        selectorAnimator.Play("Previous");

                    if (saveValue == true)
                        PlayerPrefs.SetInt(selectorTag + "HSelectorValue", index);
                }
            }

            else
            {
                labeHelper.text = label.text;

                if (index == 0)
                    index = itemList.Count - 1;

                else
                    index--;

                label.text = itemList[index].itemTitle;
                itemList[index].onValueChanged.Invoke();

                selectorAnimator.Play(null);
                selectorAnimator.StopPlayback();

                if (invertAnimation == true)
                    selectorAnimator.Play("Forward");
                else
                    selectorAnimator.Play("Previous");

                if (saveValue == true)
                    PlayerPrefs.SetInt(selectorTag + "HSelectorValue", index);
            }
        }

        public void ForwardClick()
        {
            if (loopSelection == false)
            {
                if (index != itemList.Count - 1)
                {
                    labeHelper.text = label.text;

                    if ((index + 1) >= itemList.Count)
                        index = 0;

                    else
                        index++;

                    label.text = itemList[index].itemTitle;
                    itemList[index].onValueChanged.Invoke();

                    selectorAnimator.Play(null);
                    selectorAnimator.StopPlayback();

                    if (invertAnimation == true)
                        selectorAnimator.Play("Previous");
                    else
                        selectorAnimator.Play("Forward");

                    if (saveValue == true)
                        PlayerPrefs.SetInt(selectorTag + "HSelectorValue", index);
                }
            }

            else
            {
                labeHelper.text = label.text;

                if ((index + 1) >= itemList.Count)
                    index = 0;

                else
                    index++;

                label.text = itemList[index].itemTitle;
                itemList[index].onValueChanged.Invoke();

                selectorAnimator.Play(null);
                selectorAnimator.StopPlayback();

                if (invertAnimation == true)
                    selectorAnimator.Play("Previous");
                else
                    selectorAnimator.Play("Forward");

                if (saveValue == true)
                    PlayerPrefs.SetInt(selectorTag + "HSelectorValue", index);
            }
        }
    }
}