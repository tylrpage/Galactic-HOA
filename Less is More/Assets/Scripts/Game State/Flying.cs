
using System.Collections;
using UnityEngine;

public class Flying : State
{
    public Flying(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // Lift up map from ground
        // drop leaves from sky
        Debug.Log("Flying");
        yield break;
    }

    public override void Update()
    {
        // spawn some leaves
        return;
    }

    public override IEnumerator End()
    {
        // transition to Flying
        yield break;
    }
}