using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafController : MonoBehaviour
{
    public Transform ShadowTransform;
    public Transform SpriteTransform;
    public AnimationCurve FallCurve;

    public float HeightInAir;

    private Vector3 _originalSpriteLocal;
    private Vector3 _originalShadowScale;

    private void Start()
    {
        _originalSpriteLocal = SpriteTransform.localPosition;
        _originalShadowScale = ShadowTransform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        SpriteTransform.localPosition = _originalSpriteLocal + HeightInAir * Vector3.up;
        float growthFactor = (1 + HeightInAir / 2f);
        ShadowTransform.localScale = new Vector3(_originalShadowScale.x * growthFactor, _originalShadowScale.y * growthFactor, 1);
    }
}
