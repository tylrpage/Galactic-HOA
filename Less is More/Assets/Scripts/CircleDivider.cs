using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDivider : MonoBehaviour
{
    public int Segments { get; private set; }
    
    [SerializeField] private GameObject dividerPrefab;
    private List<Transform> _activeDividers;
    private Transform _lastDivider;

    private void Awake()
    {
        Segments = 1;
        _activeDividers = new List<Transform>();
    }

    public void AddSegment()
    {
        Segments++;
        
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
