
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
        
        yield break;
    }

    public override void Update()
    {
        // if players in circle >= 2, start timer to start round
        if (_stateMachine.IsServer)
        {
            var categorized = _stateMachine.GetCategoriesOfPlayers();

            if (categorized.OnCircle.Count >= 2)
            {
                _stateMachine.SetState(new RoundStarting(_stateMachine));
            }
            else
            {
                _stateMachine.StatusTextController.SetWaitingForPlayers(categorized.OnCircle.Count);
            }
        }
        
        return;
    }
}