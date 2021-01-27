
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Landing : State
{
    public Landing(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // set status text
        
        // land the circle
        IEnumerable<Transform> groundedPlayers;
        if (_stateMachine.IsServer)
        {
            groundedPlayers = _stateMachine.GameServer._peerDatas.Values.Where(x => !x.IsPlaying).Select(x => x.PlayerTransform);
            
        }
        else
        {
            groundedPlayers = _stateMachine.GameClient._peerDatas.Values.Where(x => !x.IsPlaying)
                .Select(x => x.PlayerTransform);
        }
        Debug.Log("Land");
        _stateMachine.DoCoroutine(_stateMachine.GroundControl.Land(groundedPlayers));
        
        
        yield break;
    }

    public override void Update()
    {
        // if players in circle >= 2, start timer to end
        return;
    }
}