
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Waiting : State
{
    private bool _liftingOff;
    
    public Waiting(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // set status text
        yield break;
    }

    public override void Update()
    {
        // if players in circle >= 2, start timer to end
        if (_stateMachine.IsServer)
        {
            if (!_liftingOff)
            {
                IEnumerable<Transform> playerTransform = _stateMachine.GameServer._peerDatas.Values.Select(x => x.PlayerTransform);
                var categorized = _stateMachine.GroundControl.CategorizePlayers(playerTransform);
                if (categorized.OnCircle.Count >= 2)
                {
                    _stateMachine.DoCoroutine(_stateMachine.GroundControl.LiftOff(categorized.OffCircle));
                    _stateMachine.GroundControl.EnableBorder();
                    _liftingOff = true;
                    _stateMachine.DoCoroutine(SwitchToFlying());
                }
            }
        }
        
        return;
    }

    private IEnumerator SwitchToFlying()
    {
        yield return new WaitForSeconds(3f);
        _stateMachine.SetState(new Flying(_stateMachine));
    }
}