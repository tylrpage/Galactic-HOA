using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform _cameraTransform;
    private Transform _myTransform;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        _myTransform.forward = -(_cameraTransform.position - transform.position);
    }
}
