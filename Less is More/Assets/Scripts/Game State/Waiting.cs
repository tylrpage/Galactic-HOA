
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Waiting : State
{
    private Coroutine _waitToStartRoundCoroutine;
    
    public Waiting(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // set status text
        var categorized = _stateMachine.GetCategoriesOfPlayers();
        _stateMachine.StatusTextController.SetWaitingForPlayers(categorized.OnCircle.Count);
        
        _stateMachine.GroundControl.DisableBorder();
        _stateMachine.CircleDivider.SetSegments(0);
        _stateMachine.CircleDivider.SetArrowsSegment(-1);

        // TODO: Push everyone out
        
        yield break;
    }

    public override void Update()
    {
        // if players in circle >= 2, start timer to start round
        var categorized = _stateMachine.GetCategoriesOfPlayers();
        if (_stateMachine.IsServer)
        {
            if (categorized.OnCircle.Count >= Constants.PLAYER_NEEDED)
            {
                _stateMachine.SetState(new RoundStarting(_stateMachine));
            }
            else
            {
                _stateMachine.StatusTextController.SetWaitingForPlayers(categorized.OnCircle.Count);
            }
        }
        else
        {
            _stateMachine.StatusTextController.SetWaitingForPlayers(categorized.OnCircle.Count);
        }
        
        return;
    }
}