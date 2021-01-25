using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDivider : MonoBehaviour
{
    private struct DividerInfo
    {
        public Quaternion originalRot;
        public Quaternion targetRot;
        public Transform dividerTransform;
    }
    
    public short Segments { get; private set; }
    public AnimationCurve DividerCurve;

#pragma warning disable 0649 
    [SerializeField] private GameObject dividerPrefab;
#pragma warning restore 0649 
    
    private List<DividerInfo> _activeDividers;
    private Transform _lastDivider;
    private float _t = 0;

    private void Awake()
    {
        Segments = 0;
        _activeDividers = new List<DividerInfo>();
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

    private void Update()
    {
        for (int i = 0; i < _activeDividers.Count; i++)
        {
            DividerInfo divider = _activeDividers[i];
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, (360f / Segments) * i));

            divider.dividerTransform.rotation = Quaternion.Lerp(divider.originalRot, divider.targetRot,
                NonLinearTransforms.BounceClampTop(DividerCurve.Evaluate(_t)));
        }
        _t += Time.deltaTime;
    }

    public void AddSegment()
    {
        _t = 0;
        
        Segments++;

        // GUARD, don't do anything for the first segment created
        if (Segments == 1)
            return;
        
        if (Segments == 2)
        {
            CreateNewDivider(Quaternion.identity, ref _activeDividers, out _lastDivider);
        }
        
        CreateNewDivider(_lastDivider.rotation, ref _activeDividers, out _lastDivider);

        UpdateTargetRots(ref _activeDividers);
    }

    public void RemoveSegment()
    {
        _t = 0;
        
        // GUARD from going to less than 1 segment
        if (Segments == 1)
            return;

        Segments--;
        if (Segments == 1)
        {
            foreach (var divider in _activeDividers)
            {
                Destroy(divider.dividerTransform.gameObject);
            }
            _activeDividers.Clear();
        }
        else
        {
            _activeDividers.RemoveAt(_activeDividers.Count - 1);
            Destroy(_lastDivider.gameObject);
            _lastDivider = _activeDividers[_activeDividers.Count - 1].dividerTransform;
        }

        UpdateTargetRots(ref _activeDividers);
    }

    // Create another divider on top of our last one
    private GameObject CreateNewDivider(Quaternion rotation, ref List<DividerInfo> activeDividers, out Transform lastDivider)
    {
        Transform newDivider = Instantiate(dividerPrefab, Vector3.zero, rotation).transform;
        newDivider.parent = gameObject.transform;
        newDivider.localPosition = Vector3.zero;
        
        DividerInfo dividerInfo = new DividerInfo()
        {
            originalRot = rotation,
            dividerTransform = newDivider
        };
        
        activeDividers.Add(dividerInfo);
        lastDivider = newDivider;

        return newDivider.gameObject;
    }

    private void UpdateTargetRots(ref List<DividerInfo> infos)
    {
        int count = infos.Count;
        for (int i = 0; i < count; i++)
        {
            DividerInfo newInfo = infos[i];
            newInfo.originalRot = newInfo.dividerTransform.rotation;
            newInfo.targetRot = Quaternion.Euler(new Vector3(0, 0, (360f / Segments) * i + (Segments * 40)));
            infos[i] = newInfo;
        }
    }
}
