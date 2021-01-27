using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafController : MonoBehaviour
{
    public Transform ShadowTransform;
    public Transform SpriteTransform;
    public AnimationCurve FallCurve;
    public float FallFactor;
    public float MaxHeightToEnableCollider;
    public bool Simulate = false;

    public float HeightInAir;

    private Vector3 _originalSpriteLocal;
    private Vector3 _originalShadowScale;
    private CircleCollider2D _circleCollider;
    private float _t;

    private void Start()
    {
        _originalSpriteLocal = SpriteTransform.localPosition;
        _originalShadowScale = ShadowTransform.localScale;
        _circleCollider = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Simulate)
            SimulateFrame();
        
        SpriteTransform.localPosition = _originalSpriteLocal + HeightInAir * Vector3.up;
        float growthFactor = (1 + HeightInAir / 2f);
        ShadowTransform.localScale = new Vector3(_originalShadowScale.x * growthFactor, _originalShadowScale.y * growthFactor, 1);
    }

    private void SimulateFrame()
    {
        // Make leaf fall
        if (HeightInAir > 0)
        {
            _t += Time.deltaTime;
            _t %= 1f;
            HeightInAir -= FallCurve.Evaluate(_t) / FallFactor;
        }
        else
        {
            HeightInAir = 0;
            _t = 0;
        }
        
        if (HeightInAir > MaxHeightToEnableCollider)
        {
            _circleCollider.enabled = true;
        }
    }
}
