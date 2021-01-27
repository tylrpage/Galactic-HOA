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
    [SerializeField] private GameObject arrow;
#pragma warning restore 0649
    
    private List<DividerInfo> _activeDividers;
    private Transform _lastDivider;
    private float _t = 0;
    private short _mySegment = -1;
    private bool _oneToTwo = false; // Used by the arrow
    private Vector3 _arrowOriginalPos;
    private Vector3 _arrowTarget;

    private void Awake()
    {
        Segments = 0;
        _activeDividers = new List<DividerInfo>();
    }
    
    public float GetAngleOfFirstDivider()
    {
        // if (Segments < 2)
        //     return 0;
        //
        // return _activeDividers[0].dividerTransform.rotation.eulerAngles.z;

        return (40 * Segments) % 360;
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

    public void SetArrowsSegment(short segment)
    {
        _mySegment = segment;
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

        if (Segments > 1)
        {
            arrow.transform.position = Vector3.Slerp(_arrowOriginalPos, _arrowTarget,
                NonLinearTransforms.BounceClampTop(DividerCurve.Evaluate(_t)));
            arrow.transform.rotation = Quaternion.FromToRotation(Vector3.up, -arrow.transform.position);
        }
        
        _t += Time.deltaTime;
        _t = Mathf.Min(_t, 1);
    }

    public void AddSegment()
    {
        _t = 0;
        
        Segments++;

        // GUARD, don't do anything for the first segment created
        if (Segments == 1)
            return;
        else
        {
            arrow.SetActive(true);
        }
        
        if (Segments == 2)
        {
            CreateNewDivider(Quaternion.identity, ref _activeDividers, out _lastDivider);
            _oneToTwo = true;
        }
        else
        {
            _oneToTwo = false;
        }
        
        CreateNewDivider(_lastDivider.rotation, ref _activeDividers, out _lastDivider);

        UpdateTargetRots(ref _activeDividers);

        _arrowOriginalPos = arrow.transform.position;
        _arrowTarget = CalcArrowTarget(Segments);
    }

    private Vector3 CalcArrowTarget(short segments)
    {
        if (_mySegment < 0)
            return Vector3.zero;

        float arrowAngle = _activeDividers[_mySegment].targetRot.eulerAngles.z + ((360f / segments) / 2f);
        return MathUtils.PolarToRect(MathUtils.DegreeToRadians(arrowAngle), 4);
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
            arrow.SetActive(false);
            
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
            
            UpdateTargetRots(ref _activeDividers);
        
            _arrowOriginalPos = arrow.transform.position;
            _arrowTarget = CalcArrowTarget(Segments);
        }
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
