using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafInterp : MonoBehaviour
{
    public Transform SpriteTransform;
    
    private Vector2 _to;
    private float _toHeight;
    private float _t;
    private int _pushes;
    private LeafController _leafController;

    private void Start()
    {
        _leafController = GetComponent<LeafController>();
    }

    public void PushNewTo(Vector2 newTo, float newHeightInAir)
    {
        _t = 0;
        _toHeight = newHeightInAir;
        _to = newTo;

        if (_pushes < 2)
        {
            _pushes++;
        }
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
        _to = pos;
        _pushes = 2;
    }

    public void SetRotation(Quaternion rotation)
    {
        SpriteTransform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (_pushes >= 2)
        {
            transform.position = Vector3.Lerp(transform.position, _to, _t);
            _leafController.HeightInAir = Mathf.Lerp(_leafController.HeightInAir, _toHeight, _t);
        }
        _t += Time.deltaTime * Constants.TICK * 0.5f;
    }
}
