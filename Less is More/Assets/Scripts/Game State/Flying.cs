
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messages;
using UnityEngine;

public class Flying : State
{
    public Flying(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // Lift up map from ground
        // divide circle

        var categorized = _stateMachine.GetCategoriesOfPlayers();
        if (_stateMachine.IsServer)
        {
            // Spawn leafs
            _stateMachine.GameServer._leafSpawner.SpawnLeafsOverTime();
            
            // Create segments for each person
            // Tell each person their segment
            _stateMachine.CircleDivider.SetSegments((short)categorized.OnCircle.Count);

            short nextSegmentToAssign = 0;
            foreach (var keyValue in _stateMachine.GameServer._peerDatas)
            {
                // They are playing this round if their id is in OnCircle
                bool isPlaying = categorized.OnCircle.ContainsKey(keyValue.Key);
                keyValue.Value.IsPlaying = isPlaying;
                
                ZoneCountChange zoneCountChange = new ZoneCountChange()
                {
                    NewZoneCount = (short)categorized.OnCircle.Count,
                    YourSegment = nextSegmentToAssign
                };

                if (isPlaying)
                {
                    zoneCountChange.YourSegment = nextSegmentToAssign;
                    nextSegmentToAssign++;
                }
                else
                {
                    zoneCountChange.YourSegment = -1;
                }
                
                _stateMachine.GameServer.NotifyClientOfZoneCount(keyValue.Key, zoneCountChange);
            }

            _stateMachine.DoCoroutine(WaitThenSwitchToPlaying(Constants.FLYING_LENGTH));
        }
        
        _stateMachine.DoCoroutine(_stateMachine.GroundControl.LiftOff(categorized.OffCircle.Values));
        _stateMachine.GroundControl.EnableBorder();
        _stateMachine.StatusTextController.SetFlyingCountdown(Constants.FLYING_LENGTH);

        yield break;
    }

    public override void Update()
    {
        // spawn some leaves
        return;
    }
    
    private IEnumerator WaitThenSwitchToPlaying(short time)
    {
        yield return new WaitForSeconds(time);
        
        _stateMachine.SetState(new Playing(_stateMachine));
    }
}