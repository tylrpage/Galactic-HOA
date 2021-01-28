using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GroundControl : MonoBehaviour
{
    public Transform Ground;
    public AnimationCurve LiftCurve;
    public float TimeToLiftOff;
    public float DistanceToLiftOff;
    public float CircleRadius;
    public GameObject CircleBorder;

    private Vector3 _originalGroundPosition;
    private List<Transform> _playersNotInCircle;

    private void Start()
    {
        _originalGroundPosition = Ground.position;
        _playersNotInCircle = new List<Transform>();
    }

    public struct Categorized
    {
        public Dictionary<int, Transform> OnCircle;
        public Dictionary<int, Transform> OffCircle;
    }

    public Vector3 GetGroundOffset()
    {
        return Ground.position - _originalGroundPosition;
    }

    public Categorized CategorizePlayers(Dictionary<int, Transform> players)
    {
        var onCircle = new Dictionary<int, Transform>();
        var offCircle = new Dictionary<int, Transform>();

        Vector3 circlePos = transform.position;
        foreach (var keyValue in players)
        {
            float distance = (circlePos - keyValue.Value.position).magnitude;
            
            if (distance < CircleRadius)
                onCircle[keyValue.Key] = keyValue.Value;
            else
                offCircle[keyValue.Key] = keyValue.Value;
        }
        
        Categorized ret = new Categorized()
        {
            OffCircle = offCircle,
            OnCircle = onCircle
        };
        return ret;
    }

    public void EnableBorder()
    {
        CircleBorder.SetActive(true);
    }
    
    public void DisableBorder()
    {
        CircleBorder.SetActive(false);
    }

    public void InstantLiftOff()
    {
        Ground.position = _originalGroundPosition + DistanceToLiftOff * Vector3.up;
    }

    public void AddAnotherPlayerNotOnCircle(Transform playerNotOnCircle)
    {
        _playersNotInCircle.Add(playerNotOnCircle);
    }

    public IEnumerator LiftOff(IEnumerable<Transform> playersNotOnCircle)
    {
        _playersNotInCircle = playersNotOnCircle.ToList();
        
        // Move everyone on the ground to a lower sprite layer
        foreach (var player in _playersNotInCircle)
        {
            player.GetComponentInChildren<SortingGroup>().sortingLayerName = "GroundedPlayer";
        }
        
        float t = 0;

        while (t <= 1)
        {
            float groundOffset = -(LiftCurve.Evaluate(t + Time.deltaTime / TimeToLiftOff) - LiftCurve.Evaluate(t)) * DistanceToLiftOff;
            Ground.position += groundOffset * Vector3.up;

            foreach (var player in _playersNotInCircle)
            {
                if (player != null)
                    player.position += groundOffset * Vector3.up;
            }
            
            t += Time.deltaTime / TimeToLiftOff;
            yield return null;
        }
    }

    public IEnumerator Land()
    {
        float t = 0;
        
        while (t <= 1)
        {
            float groundOffset = (LiftCurve.Evaluate(t + Time.deltaTime / TimeToLiftOff) - LiftCurve.Evaluate(t)) * DistanceToLiftOff;
            Ground.position += groundOffset * Vector3.up;

            foreach (var player in _playersNotInCircle)
            {
                player.position += groundOffset * Vector3.up;
            }
            
            t += Time.deltaTime / TimeToLiftOff;
            yield return null;
        }

        // After landing is complete, move ground to exactly the right position
        Ground.position = _originalGroundPosition;
        
        // Move everyone on the ground back to their normal sorting layer
        foreach (var player in _playersNotInCircle)
        {
            player.GetComponentInChildren<SortingGroup>().sortingLayerName = "Default";
        }
    }
}
