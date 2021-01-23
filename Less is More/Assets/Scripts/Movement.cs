using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speedMult;

    private Rigidbody2D _rb;
    private AnimationController _animationController;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animationController = GetComponent<AnimationController>();
    }

    private void FixedUpdate()
    {
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _rb.AddForce(inputDir * speedMult, ForceMode2D.Force);
        
        // Animation stuff
        if (inputDir == Vector2.zero)
        {
            _animationController.ChangeAnimationState("idle");
        }
        else
        {
            _animationController.ChangeAnimationState("run");

            if (inputDir.x > 0)
            {
                _animationController.SetSpriteDirection(true);
            }
            else if (inputDir.x < 0)
            {
                _animationController.SetSpriteDirection(false);
            }
        }
    }
}
