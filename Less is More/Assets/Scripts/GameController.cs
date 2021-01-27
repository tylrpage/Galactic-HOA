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
    public bool nonCloneIsServer = false;
    public bool connectToRemote = false;

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
            if (nonCloneIsServer)
                StartServer();
            else
                StartClient();
        }
        else
        {
            string arg = ClonesManager.GetArgument();
            if (arg.Equals("server"))
                StartServer();
            else
                StartClient();
        }
        #else
        if (Application.isBatchMode)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
        #endif
    }

    private void StartClient()
    {
        Client client = gameObject.AddComponent<Client>();
        client.Connect(connectToRemote);
    }

    private void StartServer()
    {
        gameObject.AddComponent<Server>();
    }
}
