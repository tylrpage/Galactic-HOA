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

    public void ChangeAnimationState(string newState)
    {
        if (CurrentAnimationState == newState) return;
        
        animator.Play(newState);
        CurrentAnimationState = newState;
    }
}
