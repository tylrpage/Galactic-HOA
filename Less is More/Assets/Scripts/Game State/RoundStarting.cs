
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundStarting : State
{
    private Coroutine _waitToStartRoundCoroutine;
    
    public RoundStarting(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // set status text
        var categorized = _stateMachine.GetCategoriesOfPlayers();
        _stateMachine.StatusTextController.SetRoundAboutToStart(Constants.ROUND_BEGIN);
        _waitToStartRoundCoroutine = _stateMachine.DoCoroutine(WaitToStartRound());
        
        yield break;
    }

    public override void Update()
    {
        // if players in circle >= 2, start timer to start round
        if (_stateMachine.IsServer)
        {
            var categorized = _stateMachine.GetCategoriesOfPlayers();

            if (categorized.OnCircle.Count < 2)
            {
                _stateMachine.CancelCoroutine(_waitToStartRoundCoroutine);
                _stateMachine.StatusTextController.CancelRoundStart();
                _stateMachine.SetState(new Waiting(_stateMachine));
            }
        }
        
        return;
    }

    private IEnumerator WaitToStartRound()
    {
        yield return new WaitForSeconds(Constants.ROUND_BEGIN);
        _stateMachine.SetState(new Flying(_stateMachine));
    }
}