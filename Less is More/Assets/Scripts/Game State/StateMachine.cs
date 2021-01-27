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
            CircleDivider.RemoveSegment();

            short nextSegmentToAssign = 0;
            foreach (var keyValue in GameServer._peerDatas)
            {
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

    private void Update()
    {
        if (State != null)
            State.Update();
    }
}
