using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInterp : MonoBehaviour
{
    private Vector3 _from;
    private Vector3 _to;
    private float _t;
    private byte _pushes;

    public void PushNewTo(Vector3 newTo)
    {
        _t = 0;
        _from = _to;
        _to = newTo;

        if (_pushes < 2)
        {
            _pushes++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_pushes >= 2)
        {
            transform.position = Vector3.Lerp(_from, _to, _t);
        }
        
        _t += Time.deltaTime * Constants.TICK;
    }
}
