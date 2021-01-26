
using System.Collections;
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
        // drop leaves from sky

        var categorized = _stateMachine.GetCategoriesOfPlayers();
        if (_stateMachine.IsServer)
        {
            // Decide who is on the circle and in this round (playing or watching)
            // Create segments for each person
            // Tell each person their segment
            
            
            _stateMachine.CircleDivider.SetSegments((short)categorized.OnCircle.Count);

            short nextSegmentToAssign = 0;
            foreach (var keyValue in _stateMachine.GameServer._peerDatas)
            {
                // They are playing this round if their id is in OnCircle
                keyValue.Value.IsPlaying = categorized.OnCircle.ContainsKey(keyValue.Key);
                
                ZoneCountChange zoneCountChange = new ZoneCountChange()
                {
                    NewZoneCount = (short)categorized.OnCircle.Count,
                    YourSegment = nextSegmentToAssign
                };
                _stateMachine.GameServer.NotifyClientOfZoneCount(keyValue.Key, zoneCountChange);

                nextSegmentToAssign++;
            }
        }
        
        _stateMachine.DoCoroutine(_stateMachine.GroundControl.LiftOff(categorized.OffCircle.Values));
        _stateMachine.GroundControl.EnableBorder();

        yield break;
    }

    public override void Update()
    {
        // spawn some leaves
        return;
    }
}