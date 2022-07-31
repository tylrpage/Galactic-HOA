using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInterp : MonoBehaviour
{
    private Vector2 _from;
    private Vector2 _to;
    private float _t;
    private int _pushes;

    public void PushNewTo(Vector2 newTo)
    {
        _t = 0;
        _from = transform.position;
        _to = newTo;

        if (_pushes < 2)
        {
            transform.position = newTo;
            _pushes++;
        }
        
        Debug.Log("RESET");
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
        _to = pos;
        _pushes = 2;
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        _t += Time.deltaTime * Constants.TICK;
        
        if (_pushes >= 2)
        {
            transform.position = Vector3.Lerp(_from, _to, _t);
        }
    }
}
