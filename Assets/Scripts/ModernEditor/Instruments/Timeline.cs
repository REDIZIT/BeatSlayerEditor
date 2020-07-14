using InGame.Game.Spawn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timeline : MonoBehaviour
{
    public EditorBeatManager bm;
    public AudioSource asource;

    public Slider slider;
    public Image playPauseImage;
    public Sprite playSprite, pauseSprite;

    public Text timeText, speedText;

    public InputField bpmField;

    bool isSliderChanging;


    private void Start()
    {
        slider.maxValue = asource.clip.length;
        slider.value = 0;
    }
    private void Update()
    {
        isSliderChanging = true;
        slider.value = asource.time;
        isSliderChanging = false;

        timeText.text = TheGreat.SecondsToTime(asource.time) + " / " + TheGreat.SecondsToTime(asource.clip.length);
        speedText.text = "x" + (asource.pitch == 1 ? "1,0" : asource.pitch + "");


        //float timeUntilEnd = asource.clip.length - asource.time;
        //if(timeUntilEnd - asource.pitch <= 0 && !asource.isPlaying)
        //{
        // ended
        //}

        playPauseImage.sprite = asource.isPlaying ? pauseSprite : playSprite;
        //if (!asource.isPlaying && playPauseImage.sprite != playSprite) playPauseImage.sprite = playSprite;
    }
    
    public void Seek(float time, bool relative = false)
    {
        if (!relative) asource.time = time;
        else
        {
            float absoluteTime = asource.time + time;
            absoluteTime = Mathf.Clamp(absoluteTime, 0, asource.clip.length);
            asource.time = absoluteTime;
        }
    }

    public void SeekRelative(float time)
    {
        Seek(time, true);
    }
    
    

    public void OnSliderChange()
    {
        if (isSliderChanging) return;
        asource.time = slider.value;
    }
    
    public void OnPlayPauseBtnClick()
    {
        if (asource.isPlaying) Pause();
        else Resume();
    }
    public void SlowSpeed()
    {
        if (asource.pitch == 1) asource.pitch = 0.7f;
        else asource.pitch = 0.4f;
    }
    public void NormalSpeed()
    {
        asource.pitch = 1;
    }

    public void Pause()
    {
        asource.Pause();
        playPauseImage.sprite = playSprite;
    }
    public void Resume()
    {
        asource.Play();
        playPauseImage.sprite = pauseSprite;
    }

    public void OnBpmChange()
    {
        if(bpmField.text == "")
        {
            bm.BPM = 0;
            return;
        }
        if(int.TryParse(bpmField.text, out int bpm))
        {
            bm.BPM = bpm;
        }
    }
}