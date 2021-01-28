using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using ParrelSync;
#endif

using UnityEngine;

public class GameController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CircleDivider circleDivider;
#pragma warning restore 0649

    public Transform SpawnPoint;
    public bool NonCloneIsServer = false;
    public bool ConnectToRemote = false;

    public GameObject GetPlayerPrefab()
    {
        return playerPrefab;
    }

    public CircleDivider GetCircleDivider()
    {
        return circleDivider;
    }
    
    private void Start()
    {
        #if UNITY_EDITOR
        if (!ClonesManager.IsClone())
        {
            if (NonCloneIsServer)
                StartServer();
            else
                StartClient(ConnectToRemote);
        }
        else
        {
            string arg = ClonesManager.GetArgument();
            if (arg.Equals("server"))
                StartServer();
            else
                StartClient(ConnectToRemote);
        }
        #else
        if (Application.isBatchMode)
        {
            StartServer();
        }
        else
        {
            StartClient(true);
        }
        #endif
    }

    private void StartClient(bool connectToRemote)
    {
        Client client = gameObject.AddComponent<Client>();
        client.Connect(connectToRemote);
    }

    private void StartServer()
    {
        gameObject.AddComponent<Server>();
    }
}
