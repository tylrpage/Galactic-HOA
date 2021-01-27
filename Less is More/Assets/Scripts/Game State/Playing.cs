
using System.Collections;
using UnityEngine;

public class Playing : State
{
    public Playing(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // set status text
        _stateMachine.StatusTextController.SetRoundCountdown(Constants.ROUND_LENGTH);

        if (_stateMachine.IsServer)
        {
            _stateMachine.DoCoroutine(WaitThenSwitchToLanding(Constants.ROUND_LENGTH));
        }

        yield break;
    }

    public override void Update()
    {
        return;
    }

    private IEnumerator WaitThenSwitchToLanding(short time)
    {
        yield return new WaitForSeconds(time);
        _stateMachine.SetState(new Landing(_stateMachine));
    }
}