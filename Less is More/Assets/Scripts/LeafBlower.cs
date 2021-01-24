using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafBlower : MonoBehaviour
{
    private static readonly int MaxLeafsThatCanBeBlown = 20;
    
    [SerializeField] private PolygonCollider2D windCollider;
    private Inputs _inputs;
    private ContactFilter2D _filter;

    private void Awake()
    {
        int leafLayer = LayerMask.NameToLayer("Leaf");
        _filter.layerMask = (1 << leafLayer);
        _filter.useLayerMask = true;
    }

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
            
            ContactFilter2D filter = new ContactFilter2D();

            Collider2D[] colliders = new Collider2D[MaxLeafsThatCanBeBlown];
            int leafCount = windCollider.OverlapCollider(_filter, colliders);

            for (int i = 0; i < leafCount; i++)
            {
                Rigidbody2D rb = colliders[i].attachedRigidbody;
                Vector2 dir = (colliders[i].transform.position - transform.position);
                rb.AddForce(dir.normalized / dir.magnitude);
            }
        }
        else
        {
            windCollider.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
