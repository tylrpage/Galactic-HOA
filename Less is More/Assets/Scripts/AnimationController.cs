using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Animator animator;
    [SerializeField] private Transform spriteTransform;
#pragma warning restore 0649 
    
    private string _currentAnimationState;

    public void SetSpriteDirection(bool right)
    {
        Vector3 currentScale = spriteTransform.localScale;
        if (right)
        {
            spriteTransform.localScale = new Vector3(Mathf.Abs(currentScale.x) * -1f, currentScale.y, currentScale.z);
        }
        else
        {
            spriteTransform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    public void ChangeAnimationState(string newState)
    {
        if (_currentAnimationState == newState) return;
        
        animator.Play(newState);
        _currentAnimationState = newState;
    }
}
