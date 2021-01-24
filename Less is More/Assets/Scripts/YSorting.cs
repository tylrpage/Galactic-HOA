using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class YSorting : MonoBehaviour
{
    public SortingGroup SortingGroup;
    private Transform _transform;

    private void Start()
    {
        _transform = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 position = _transform.position;
        SortingGroup.sortingOrder = -1 * Mathf.RoundToInt(position.y * 100) * 10;
    }
}
