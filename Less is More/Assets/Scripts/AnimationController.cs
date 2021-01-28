using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Animator animator;
    [SerializeField] private Transform spriteTransform;
#pragma warning restore 0649 
    
    public string CurrentAnimationState { get; private set; }
    public bool SpriteFlipped { get; private set; }

    private int _faceLayerId;

    private void Awake()
    {
        _faceLayerId = animator.GetLayerIndex("Face Layer");
    }

    public void SetSpriteDirection(bool right)
    {
        Vector3 currentScale = spriteTransform.localScale;
        if (right)
        {
            SpriteFlipped = true;
            spriteTransform.localScale = new Vector3(Mathf.Abs(currentScale.x) * -1f, currentScale.y, currentScale.z);
        }
        else
        {
            SpriteFlipped = false;
            spriteTransform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    public void SetFace(bool isBlowing)
    {
        if (isBlowing)
        {
            ChangeAnimationState("face_blow", _faceLayerId);
        }
        else
        {
            ChangeAnimationState("face_neutral", _faceLayerId);
        }
    }

    public void ChangeAnimationState(string newState, int layer = 0)
    {
        if (CurrentAnimationState == newState) return;
        
        animator.Play(newState, layer);
        CurrentAnimationState = newState;
    }
}
