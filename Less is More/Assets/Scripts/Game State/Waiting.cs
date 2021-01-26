
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Waiting : State
{
    public static readonly short ID = 1;
    
    private bool _waitingForPlayers = true;
    private Coroutine _waitToStartRoundCoroutine;
    
    public Waiting(StateMachine stateMachine) : base(stateMachine)
    {
        Id = ID;
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

            if (_waitingForPlayers)
            {
                if (categorized.OnCircle.Count >= 2)
                {
                    _stateMachine.StatusTextController.SetRoundAboutToStart(Constants.ROUND_BEGIN);
                    _waitToStartRoundCoroutine = _stateMachine.DoCoroutine(WaitToStartRound());
                    _waitingForPlayers = false;
                }
                else
                {
                    _stateMachine.StatusTextController.SetWaitingForPlayers(categorized.OnCircle.Count);
                }
            }
            else if (!_waitingForPlayers && categorized.OnCircle.Count < 2)
            {
                _stateMachine.StatusTextController.SetWaitingForPlayers(categorized.OnCircle.Count);
                _stateMachine.StatusTextController.CancelRoundStart();
                _stateMachine.CancelCoroutine(_waitToStartRoundCoroutine);
                _waitingForPlayers = true;
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