using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDivider : MonoBehaviour
{
    public short Segments { get; private set; }

#pragma warning disable 0649 
    [SerializeField] private GameObject dividerPrefab;
#pragma warning restore 0649 
    
    private List<Transform> _activeDividers;
    private Transform _lastDivider;

    private void Awake()
    {
        Segments = 0;
        _activeDividers = new List<Transform>();
    }

    public void SetSegments(short newCount)
    {
        int diff = newCount - Segments;
        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                AddSegment();
            }
        }
        else if (diff < 0)
        {
            for (int i = 0; i < Mathf.Abs(diff); i++)
            {
                RemoveSegment();
            }
        }
    }

    public void AddSegment()
    {
        Segments++;

        // GUARD, don't do anything for the first segment created
        if (Segments == 1)
            return;
        
        if (Segments == 2)
        {
            CreateNewDivider(Quaternion.identity, ref _activeDividers, out _lastDivider);
        }
        
        CreateNewDivider(_lastDivider.rotation, ref _activeDividers, out _lastDivider);

        for (int i = 0; i < _activeDividers.Count; i++)
        {
            Transform divider = _activeDividers[i];
            divider.rotation = Quaternion.Euler(new Vector3(0, 0, (360f / Segments) * i));
        }
    }

    public void RemoveSegment()
    {
        // GUARD from going to less than 1 segment
        if (Segments == 1)
            return;

        Segments--;
        if (Segments == 1)
        {
            foreach (var divider in _activeDividers)
            {
                Destroy(divider.gameObject);
            }
            _activeDividers.Clear();
        }
        else
        {
            _activeDividers.RemoveAt(_activeDividers.Count - 1);
            Destroy(_lastDivider.gameObject);
            _lastDivider = _activeDividers[_activeDividers.Count - 1];
            
            for (int i = 0; i < _activeDividers.Count; i++)
            {
                Transform divider = _activeDividers[i];
                divider.rotation = Quaternion.Euler(new Vector3(0, 0, (360f / Segments) * i));
            }
        }
    }

    // Create another divider on top of our last one
    private GameObject CreateNewDivider(Quaternion rotation, ref List<Transform> activeDividers, out Transform lastDivider)
    {
        Transform newDivider = Instantiate(dividerPrefab, Vector3.zero, rotation).transform;
        newDivider.parent = gameObject.transform;
        newDivider.localPosition = Vector3.zero;
        activeDividers.Add(newDivider);
        lastDivider = newDivider;

        return newDivider.gameObject;
    }
}
