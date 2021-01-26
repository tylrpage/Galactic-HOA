using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GroundControl : MonoBehaviour
{
    public Transform Ground;
    public AnimationCurve LiftCurve;
    public float TimeToLiftOff;
    public float DistanceToLiftOff;
    public float CircleRadius;

    public struct Categorized
    {
        public List<Transform> OnCircle;
        public List<Transform> OffCircle;
    }

    public Categorized CategorizePlayers(IEnumerable<Transform> players)
    {
        List<Transform> onCircle = new List<Transform>();
        List<Transform> offCircle = new List<Transform>();

        Vector3 circlePos = transform.position;
        foreach (var player in players)
        {
            float distance = (circlePos - player.position).magnitude;
            
            if (distance < CircleRadius)
                onCircle.Add(player);
            else
                offCircle.Add(player);
        }
        
        Categorized ret = new Categorized()
        {
            OffCircle = offCircle,
            OnCircle = onCircle
        };
        return ret;
    }

    public IEnumerator LiftOff(List<Transform> playersNotOnCircle)
    {
        // Move everyone on the ground to a lower sprite layer
        Ground.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";
        foreach (var player in playersNotOnCircle)
        {
            player.GetComponentInChildren<SortingGroup>().sortingLayerName = "GroundedPlayer";
        }
        
        float t = 0;

        while (t <= 1)
        {
            float groundOffset = -(LiftCurve.Evaluate(t + Time.deltaTime / TimeToLiftOff) - LiftCurve.Evaluate(t)) * DistanceToLiftOff;
            Ground.position += groundOffset * Vector3.up;

            foreach (var player in playersNotOnCircle)
            {
                player.position += groundOffset * Vector3.up;
            }
            
            t += Time.deltaTime / TimeToLiftOff;
            yield return null;
        }
    }
}
