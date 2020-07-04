using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AudioClipSeri : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //float[] samples = new float[clip.samples * clip.channels];
        //clip.GetData(samples, 0);
        //bsp.samples = samples;
        //bsp.clip = clip;

        bsp.bytes = File.ReadAllBytes(Application.persistentDataPath + "/AudioFiles/Vanic Ft. Katy Tiz-Samurai.ogg");

        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create("123.BSP"))
        {
            binaryFormatter.Serialize(fileStream, bsp);
        }
    }

    public AudioClip clip;
    public BSP bsp;

    public bool doTest;
    // Update is called once per frame
    void Update()
    {
        if (doTest)
        {
            doTest = false;
            //GetComponent<AudioSource>().clip.
            //GetComponent<AudioSource>().clip = audioClip;
            GetComponent<AudioSource>().Play();
        }
    }
}


[Serializable]
public class BSP
{
    public byte[] bytes;
}