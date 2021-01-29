using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteController : MonoBehaviour
{
    public Sprite Muted;
    public Sprite Unmuted;
    public Image ButtonImage;
    
    private bool isMuted = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnButtonPress()
    {
        if (isMuted)
        {
            isMuted = false;
            ButtonImage.sprite = Unmuted;
            AudioListener.pause = false;
        }
        else
        {
            isMuted = true;
            ButtonImage.sprite = Muted;
            AudioListener.pause = true;
        }
    }
}
