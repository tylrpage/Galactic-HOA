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
    
    public State State { get; private set; }
    public bool IsServer { get; private set; }

    [NonSerialized] public Server GameServer;
    [NonSerialized] public Client GameClient;

    public void Init(Server gameServer)
    {
        IsServer = true;
        GameServer = gameServer;
    }
    
    public void Init(Client gameClient)
    {
        IsServer = false;
        GameClient = gameClient;
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

    public GroundControl.Categorized GetCategoriesOfPlayers()
    {
        IEnumerable<Transform> playerTransform = GameServer._peerDatas.Values.Select(x => x.PlayerTransform);
        var categorized = GroundControl.CategorizePlayers(playerTransform);
        return categorized;
    }

    public void SetState(State state)
    {
        State = state;
        StartCoroutine(state.Start());
    }

    public short GetStateId()
    {
        if (State is Waiting)
            return 1;
        if (State is Flying)
            return 2;
        if (State is Playing)
            return 3;
        if (State is RoundStarting)
            return 4;
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
        }
    }

    private void Update()
    {
        if (State != null)
            State.Update();
    }
}
