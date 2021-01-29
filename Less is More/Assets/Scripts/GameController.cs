using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using ParrelSync;
#endif

using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CircleDivider circleDivider;
#pragma warning restore 0649

    public Color[] PossiblePlayerColors;
    public string PlayerName { get; private set; }
    
    public ConnectUIController ConnectUiController;
    public ScoreController ScoreController;
    public Transform SpawnPoint;
    public bool NonCloneIsServer = false;
    public bool ConnectToRemote = false;

    public ushort GetRandomPlayerColorCode()
    {
        ushort count = (ushort)PossiblePlayerColors.Length;
        ushort randomIndex = (ushort)Random.Range(0, count);
        return randomIndex;
    }

    public Color ConvertColorCodeToColor(ushort code)
    {
        return PossiblePlayerColors[code];
    }

    public GameObject GetPlayerPrefab()
    {
        return playerPrefab;
    }

    public CircleDivider GetCircleDivider()
    {
        return circleDivider;
    }

    private void Awake()
    {
        ConnectUiController.ConnectPressed += ConnectUiControllerOnConnectPressed;
    }

    private void ConnectUiControllerOnConnectPressed(string name)
    {
        PlayerName = name;
        #if UNITY_EDITOR
        if (!ClonesManager.IsClone())
        {
            if (NonCloneIsServer)
                StartServer();
            else
                StartClient(ConnectToRemote, name);
        }
        else
        {
            string arg = ClonesManager.GetArgument();
            if (arg.Equals("server"))
                StartServer();
            else
                StartClient(ConnectToRemote, name);
        }
        #else
        StartClient(true, name);
        #endif
    }

    private void Start()
    {
        ConnectUiController.ShowConnectScreen();
        
        #if UNITY_EDITOR
        if (!ClonesManager.IsClone() && NonCloneIsServer)
        {
            StartServer();
        }
        else
        {
            string arg = ClonesManager.GetArgument();
            if (arg.Equals("server"))
                StartServer();
        }
        #endif
        if (Application.isBatchMode)
        {
            StartServer();
        }
    }

    private void StartClient(bool connectToRemote, string displayName)
    {
        Client client = gameObject.AddComponent<Client>();
        client.Connect(connectToRemote, displayName);

        client.Connected += () => ConnectUiController.HideConnectScreen();
        client.Disconnected += () =>
        {
            ConnectUiController.ShowConnectScreen();
            ConnectUiController.SwitchToConnectButtonSprite();
            ConnectUiController.EnableConnectButton();
        };
    }

    private void StartServer()
    {
        gameObject.AddComponent<Server>();
        ConnectUiController.HideConnectScreen();
    }
}
