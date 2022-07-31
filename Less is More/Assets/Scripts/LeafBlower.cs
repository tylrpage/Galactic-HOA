using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafBlower : MonoBehaviour
{
    private static readonly int MaxLeafsThatCanBeBlown = 20;

    public bool Simulate;
    public float LiftPower;
    public Animator WindAnimator;
    
    [SerializeField] private PolygonCollider2D windCollider;
    private ContactFilter2D _filter;
    private bool _blowing = false;
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int Stop = Animator.StringToHash("Stop");
    private bool _isPressingSpace;
    private Vector2 _mouseDir;

    private void Awake()
    {
        int leafLayer = LayerMask.NameToLayer("Leaf");
        _filter.layerMask = (1 << leafLayer);
        _filter.useLayerMask = true;
    }

    public void SetInputs(bool isPressingSpace, Vector2 mouseDir)
    {
        _isPressingSpace = isPressingSpace;
        _mouseDir = mouseDir;
    }

    private void FixedUpdate()
    {
        if (Simulate)
        {
            SimulateLeafBlowing();
        }

        windCollider.transform.up = _mouseDir;
        if (_isPressingSpace)
        {
            if (!_blowing)
            {
                WindAnimator.Play("blow_start");
                _blowing = true;
            }
        }
        else if (_blowing)
        {
            WindAnimator.Play("blow_stop");
            _blowing = false;
        }
    }

    private void SimulateLeafBlowing()
    {
        if (_isPressingSpace)
        {
            ContactFilter2D filter = new ContactFilter2D();

            Collider2D[] colliders = new Collider2D[MaxLeafsThatCanBeBlown];
            int leafCount = windCollider.OverlapCollider(_filter, colliders);

            for (int i = 0; i < leafCount; i++)
            {
                LeafController leafController = colliders[i].GetComponent<LeafController>();
                if (leafController.Blowable())
                {
                    Rigidbody2D rb = colliders[i].attachedRigidbody;
                    Vector2 dir = (colliders[i].transform.position - transform.position);
                    float force = Mathf.Min(1f / Mathf.Pow(dir.magnitude, 2), Constants.MAX_LEAFBLOW_FORCE);
                    rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
                    leafController.PushUp(LiftPower / Mathf.Pow(dir.magnitude, 2));
                }
            }
        }
    }
}
