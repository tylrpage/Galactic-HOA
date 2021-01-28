using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Messages;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StateMachine : MonoBehaviour
{
    public GroundControl GroundControl;
    public StatusTextController StatusTextController;
    public GameController GameController;
    public CircleDivider CircleDivider;
    
    public State State { get; private set; }
    public bool IsServer { get; private set; }

    [NonSerialized] public Server GameServer;
    [NonSerialized] public Client GameClient;

    public void Init(Server gameServer)
    {
        IsServer = true;
        GameServer = gameServer;

        GetReferencesGameControllers();
    }
    
    public void Init(Client gameClient)
    {
        IsServer = false;
        GameClient = gameClient;
        
        GetReferencesGameControllers();
    }

    private void GetReferencesGameControllers()
    {
        GameController = GetComponent<GameController>();
        CircleDivider = GameController.GetCircleDivider();
    }

    public Coroutine DoCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public void CancelCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    public void HandlePlayerDisconnection(int id)
    {
        // GUARD, only server should be calling this
        if (!IsServer)
            throw new Exception("Only server should report this");

        if (CircleDivider.Segments > 0 && GameServer._peerDatas[id].IsPlaying)
        {
            CircleDivider.SetSegments((short) (CircleDivider.Segments - 1));

            short nextSegmentToAssign = 0;
            foreach (var keyValue in GameServer._peerDatas)
            {
                // Skip over the person disconnecting
                if (keyValue.Key == id)
                    continue;
                
                ZoneCountChange zoneCountChange = new ZoneCountChange()
                {
                    NewZoneCount = CircleDivider.Segments,
                    YourSegment = nextSegmentToAssign++
                };
                
                GameServer.NotifyClientOfZoneCount(keyValue.Key, zoneCountChange);
            }
        }
    }

    public GroundControl.Categorized GetCategoriesOfPlayers()
    {
        Dictionary<int, Transform> playerTransform;
        if (IsServer)
        {
            playerTransform = GameServer._peerDatas.ToDictionary(x => x.Key, x => x.Value.PlayerTransform);
        }
        else
        {
            playerTransform = GameClient._peerDatas.ToDictionary(x => x.Key, x => x.Value.PlayerTransform);
        }
        
        var categorized = GroundControl.CategorizePlayers(playerTransform);
        return categorized;
    }

    public void SetState(State newState)
    {
        State?.End();
        
        State = newState;
        StartCoroutine(newState.Start());

        if (IsServer)
        {
            // notify all clients of the state change
            GameServer.NotifyClientsOfStateChange(GetStateId(newState));
        }
    }

    public short GetStateId(State state)
    {
        if (state is Waiting)
            return 1;
        if (state is Flying)
            return 2;
        if (state is Playing)
            return 3;
        if (state is RoundStarting)
            return 4;
        if (state is Landing)
            return 5;
        return 0;
    }

    public void SetStateId(short id)
    {
        switch (id)
        {
            case 1:
                SetState(new Waiting(this));
                break;
            case 2:
                SetState(new Flying(this));
                break;
            case 3:
                SetState(new Playing(this));
                break;
            case 4:
                SetState(new RoundStarting(this));
                break;
            case 5:
                SetState(new Landing(this));
                break;
        }
    }

    public short GetCurrentStateId()
    {
        return GetStateId(State);
    }

    // Handle special cases for clients coming in mid game on server
    public void SetServerJoiningState(Transform peerTransform)
    {
        int stateId = GetCurrentStateId();
        
        if (stateId == 2 || stateId == 3 || stateId == 5)
            GroundControl.AddAnotherPlayerNotOnCircle(peerTransform);
    }

    // Handle special cases for clients coming in mid game on client
    public void SetOtherClientsJoiningState(short stateId, Transform playerTransform)
    {
        if (stateId == 2 || stateId == 3 || stateId == 5)
        {
            GroundControl.AddAnotherPlayerNotOnCircle(playerTransform);
        }
    }

    public void SetMyClientJoiningState(short stateId, Transform playerTransform)
    {
        if (stateId == 1 || stateId == 4 || stateId == 5)
        {
            SetState(new Waiting(this));
        }
        else if (stateId == 2 || stateId == 3)
        {
            //Flying
            GroundControl.AddAnotherPlayerNotOnCircle(playerTransform);
            GroundControl.InstantLiftOff();
            StatusTextController.SetWaitForRoundToFinishText();
        }
    }

    public bool ShouldLockNonPlayingPlayers()
    {
        short id = GetStateId(State);
        if (id == 1 || id == 5 || id == 4)
            return false;
        else
            return true;
    }

    private void Update()
    {
        if (State != null)
            State.Update();
    }
}
