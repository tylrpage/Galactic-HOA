using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafBlower : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D windCollider;
    private Inputs _inputs;
    
    public void SetInputs(Inputs inputs)
    {
        _inputs = inputs;
    }

    private void FixedUpdate()
    {
        if (_inputs.Space)
        {
            windCollider.GetComponent<SpriteRenderer>().enabled = true;
            windCollider.transform.up = _inputs.MouseDir;
        }
        else
        {
            windCollider.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
