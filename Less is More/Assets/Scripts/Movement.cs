using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speedMult;

    private Rigidbody2D _rb;
    private AnimationController _animationController;
    private LeafBlower _leafBlower;
    private PlayerSounds _playerSounds;
    private Camera _camera;
    private Vector2 _physicsInputDir;
    private ChatController _chatController;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animationController = GetComponent<AnimationController>();
        _leafBlower = GetComponent<LeafBlower>();
        _playerSounds = GetComponent<PlayerSounds>();
        _camera = Camera.main;
        
        if (_camera != null)
        {
            _chatController = _camera.GetComponent<GameController>()?.ChatController;
        }
    }

    private void FixedUpdate()
    {
        _rb.MovePosition((Vector2)transform.position + _physicsInputDir);
        _physicsInputDir = Vector2.zero;
    }

    private void Update()
    {
        Vector2 inputDir = GetInputDir();
        _physicsInputDir += inputDir * (speedMult * Time.deltaTime);
        
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
        
        Vector2 mouseDir = ScreenToPlane() - (Vector2)transform.position;

        bool blowing = !IsTyping() && Input.GetKey(KeyCode.Space);
        
        _leafBlower.SetInputs(blowing, mouseDir);
        _animationController.SetFace(blowing);
    }
    
    private Vector2 ScreenToPlane()
    {
        Vector2 viewport = _camera.ScreenToViewportPoint(Input.mousePosition);
        return new Vector2((viewport.x - 0.5f) * _camera.orthographicSize * _camera.aspect * 2, (viewport.y - 0.5f) * _camera.orthographicSize * 2);
    }

    private Vector2 GetInputDir()
    {
        if (!IsTyping())
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        else
            return Vector2.zero;
    }

    private bool IsTyping()
    {
        if (_chatController == null)
        {
            return false;
        }

        return _chatController.typing;
    }
}
