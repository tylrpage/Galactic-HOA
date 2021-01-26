using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public GroundControl GroundControl;
    
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

    public void DoCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void SetState(State state)
    {
        State = state;
        StartCoroutine(state.Start());
    }

    private void Update()
    {
        if (State != null)
            State.Update();
    }
}
