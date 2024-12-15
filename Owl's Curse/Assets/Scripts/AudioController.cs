using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public Sprite audioOn;
    public Sprite audioOff;
    public GameObject buttonAudio;

    public AudioClip clip;
    public AudioSource audio;

    private void Update()
    {

    }

    public void OnOffAudio()
    {
        if(AudioListener.volume==1)
        {
            AudioListener.volume = 0;
            buttonAudio.GetComponent<Image>().sprite=audioOff;
        }
        else
        {
            AudioListener.volume = 1;
            buttonAudio.GetComponent<Image>().sprite = audioOn;
        }
    }
}
