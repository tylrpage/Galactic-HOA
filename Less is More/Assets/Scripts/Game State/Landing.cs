
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
        _stateMachine.StatusTextController.SetLandingText();
        
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
        _stateMachine.DoCoroutine(_stateMachine.GroundControl.Land(groundedPlayers));

        // Start countdown till switching to waiting again
        if (_stateMachine.IsServer)
        {
            _stateMachine.DoCoroutine(WaitThenScoreThenSwitchToWaiting(Constants.LANDING_LENGTH));
        }
        
        yield break;
    }

    private IEnumerator WaitThenScoreThenSwitchToWaiting(short time)
    {
        yield return new WaitForSeconds(time);
        
        // Score up the leafs
        if (_stateMachine.IsServer)
        {
            CircleDivider circleDivider = _stateMachine.CircleDivider;
            List<ushort> leafCounts = _stateMachine.GameServer._leafSpawner.GetSectorLeafCounts(circleDivider.Segments,
                circleDivider.GetAngleOfFirstDivider());
            _stateMachine.GameServer.ScoreLeafCounts(leafCounts);
        }
        
        _stateMachine.SetState(new Waiting(_stateMachine));
    }
}