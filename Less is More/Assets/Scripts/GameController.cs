using System.Collections;
using System.Collections.Generic;
using ParrelSync;
using UnityEngine;

public class GameController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject playerPrefab;
#pragma warning restore 0649

    public bool nonCloneIsServer = false;

    public GameObject GetPlayerPrefab()
    {
        return playerPrefab;
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
        #endif
    }

    private void StartClient()
    {
        Client client = gameObject.AddComponent<Client>();
        client.Connect(false);
    }

    private void StartServer()
    {
        gameObject.AddComponent<Server>();
    }
}
