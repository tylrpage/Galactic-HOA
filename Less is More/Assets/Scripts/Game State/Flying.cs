
using System.Collections;
using UnityEngine;

public class Flying : State
{
    public static readonly short ID = 2;
    
    public Flying(StateMachine stateMachine) : base(stateMachine)
    {
        Id = ID;
    }
    
    public override IEnumerator Start()
    {
        // Lift up map from ground
        // drop leaves from sky
        
        var categorized = _stateMachine.GetCategoriesOfPlayers();
        _stateMachine.DoCoroutine(_stateMachine.GroundControl.LiftOff(categorized.OffCircle));
        _stateMachine.GroundControl.EnableBorder();

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