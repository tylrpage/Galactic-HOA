using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public AudioSource StepSource;
    public AudioSource BlowSource;
    
    
    private bool playingBlow;
    private bool playingStep;

    public void SetAnimation(bool pressingSpace, string currentAnimation)
    {
        if (pressingSpace && !playingBlow)
        {
            BlowSource.Play();
            playingBlow = true;
        }
        if (!pressingSpace && playingBlow)
        {
            BlowSource.Stop();
            playingBlow = false;
        }
            

        if (currentAnimation == "run" && !playingStep)
        {
            StepSource.loop = true;
            StepSource.Play();
            playingStep = true;
        }
        if (currentAnimation != "run" && playingStep)
        {
            playingStep = false;
            StepSource.Stop();
        }
    }
}
