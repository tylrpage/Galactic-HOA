using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInterp : MonoBehaviour
{
    private readonly int bufferSize = 3;
    
    private List<Vector3> _positions;
    private float _t;

    private void Awake()
    {
        _positions = new List<Vector3>();
    }

    public void PushNewTo(Vector3 newTo)
    {
        _t = 0;
        _positions.Add(newTo);

        if (_positions.Count > bufferSize)
        {
            _positions.RemoveAt(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_positions.Count >= 2)
        {
            transform.position = Vector3.Lerp(_positions[0], _positions[1], _t);
        }
        
        _t += Time.deltaTime * Constants.TICK;

        if (_t >= 1)
        {
            _t = 0;
            _positions.RemoveAt(0);
        }
    }
}
