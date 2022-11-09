using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioToggle : MonoBehaviour
{
    bool toggle = true;
    Image image;
    [SerializeField] Sprite audioOn, audioOff;
    AudioSource EverAudio;

    void Start()
    {
        image = GetComponent<Image>();
        EverAudio = GameObject.FindWithTag("DontDestroy").GetComponent<AudioSource>();
        if (DontDestroy.instance.audioOn)
        {
            AudioOn();
        }
        else
        {
            AudioOff();
        }
    }

    public void ToggleAudio()
    {
        if (toggle)
        {
            AudioOff();
        }
        else
        {
            AudioOn();
        }
    }

    void AudioOn()
    {
        EverAudio.enabled = true;
        image.sprite = audioOn;
        image.color = Color.green;
        toggle = true;
        DontDestroy.instance.audioOn = true;
    }

    void AudioOff()
    {
        EverAudio.enabled = false;
        image.sprite = audioOff;
        image.color = Color.red;
        toggle = false;
        DontDestroy.instance.audioOn = false;
    }
}
