using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Messages;
using UnityEngine;
using Mirror.SimpleWeb;
using NetStack.Quantization;
using NetStack.Serialization;

public class Server : MonoBehaviour
{
    private SimpleWebServer _webServer;
    public Dictionary<int, ServerPeerData> _peerDatas;
    private Dictionary<int, PeerState> _peerStates;
    private List<int> _connectedIds;
    private bool _listening;
    private float _timer;
    private LeafSpawner _leafSpawner;
    private GameController _gameController;
    private StateMachine _stateMachine;

    private void Awake()
    {
        _peerDatas = new Dictionary<int, ServerPeerData>();
        _gameController = GetComponent<GameController>();
        _connectedIds = new List<int>();
        _leafSpawner = GetComponent<LeafSpawner>();
        _stateMachine = GetComponent<StateMachine>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _leafSpawner.SpawnLeafsRandomly(15);
        
        _webServer = Listen();
        _stateMachine.Init(this);
        _stateMachine.SetState(new Waiting(_stateMachine));

        _webServer.onConnect += WebServerOnonConnect;
        _webServer.onData += WebServerOnonData;
        _webServer.onError += delegate(int i, Exception exception)
        {
            Debug.Log("Error: " + exception.Message);
        };
        _webServer.onDisconnect += WebServerOnonDisconnect;
    }
    
    private SimpleWebServer Listen()
    {
        SimpleWebServer webServer;
        
        SslConfig sslConfig;
        TcpConfig tcpConfig = new TcpConfig(true, 5000, 20000);
        if (Application.isBatchMode)
        {
            Debug.Log("Setting up secure server");
            sslConfig = new SslConfig(true, "cert.pfx", "", SslProtocols.Tls12);
        }
        else
        {
            Debug.Log("Setting up non secure server");
            sslConfig = new SslConfig(false, "", "", SslProtocols.Tls12);
        }

        webServer = new SimpleWebServer(10000, tcpConfig, 16 * 1024, 3000, sslConfig);
        webServer.Start(Constants.GAME_PORT);

        Debug.Log("Server started");
        _listening = true;

        return webServer;
    }

    public void NotifyClientsOfStateChange(short stateId)
    {
        if (!_listening)
            return;
        
        StateChange stateChange = new StateChange()
        {
            StateId = stateId
        };
        var bytes = Writer.SerializeToByteSegment(stateChange);
        _webServer.SendAll(_connectedIds, bytes);
    }

    public void NotifyClientOfZoneCount(int peerId, ZoneCountChange zoneCountChange)
    {
        var bytes = Writer.SerializeToByteSegment(zoneCountChange);
        _webServer.SendOne(peerId, bytes);
    }

    private void WebServerOnonConnect(int peerId)
    {
        GameObject newPlayer = Instantiate(_gameController.GetPlayerPrefab(), _gameController.SpawnPoint.position, Quaternion.identity);
        
        Movement movement = newPlayer.GetComponent<Movement>();
        movement.enabled = true;
        LeafBlower leafBlower = newPlayer.GetComponent<LeafBlower>();
        leafBlower.enabled = true;
        newPlayer.GetComponent<CircleCollider2D>().enabled = true;

        ServerPeerData newPeerData = new ServerPeerData()
        {
            Id = peerId,
            Inputs = Inputs.EmptyInputs(),
            PlayerMovement = movement,
            PlayerTransform = newPlayer.transform,
            PlayerBlower = leafBlower,
            AnimationController = newPlayer.GetComponentInChildren<AnimationController>()
        };
        _peerDatas[peerId] = newPeerData;
        
        // Send everyone a message about this player
        NewPlayer newPlayerMessage = new NewPlayer()
        {
            TheirId = peerId,
            State = GenerateSinglePeerState(newPeerData)
        };
        ArraySegment<byte> newPlayerMessageBytes = Writer.SerializeToByteSegment(newPlayerMessage);
        _webServer.SendAll(_connectedIds, newPlayerMessageBytes);
        
        // Give connecting peer his initial handshake data
        _connectedIds.Add(peerId);
        InitialState initialState = new InitialState()
        {
            States = GeneratePeerStates(_peerDatas, false),
            LeafStates = _leafSpawner.GenerateLeafStates(false),
            YourId = peerId
        };
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(initialState);
        _webServer.SendOne(peerId, bytes);
    }

    private void WebServerOnonDisconnect(int id)
    {
        // Tell everyone they are disconnecting
        PlayerDisconnected playerDisconnected = new PlayerDisconnected()
        {
            TheirId = id
        };
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(playerDisconnected);
        _webServer.SendAll(_connectedIds, bytes);
        
        // Remove circle division
        _stateMachine.HandlePlayerDisconnection(id);

        Destroy(_peerDatas[id].PlayerTransform.gameObject);
        _peerDatas.Remove(id);
        _connectedIds.Remove(id);
    }

    private void WebServerOnonData(int peerId, ArraySegment<byte> data)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        bitBuffer.FromArray(data.Array, data.Count);
        ushort messageId = bitBuffer.PeekUShort();

        switch (messageId)
        {
            // Client input
            case 2:
            {
                ClientInputs clientInputs = new ClientInputs();
                clientInputs.Deserialize(ref bitBuffer);
                _peerDatas[peerId].Inputs = clientInputs.inputs;
                _peerDatas[peerId].PlayerMovement.SetInputs(clientInputs.inputs);
                _peerDatas[peerId].PlayerBlower.SetInputs(clientInputs.inputs);

                break;
            }
        }
    }

    void Update()
    {
        _webServer.ProcessMessageQueue(this);
        
        // GUARD
        if (!_listening)
            return;
        
        _timer += Time.deltaTime;
        while (_timer >= Constants.STEP)
        {
            _timer -= Constants.STEP;

            if (_connectedIds.Count > 0)
            {
                // send states
                PeerStates peerStates = new PeerStates()
                {
                    States = GeneratePeerStates(_peerDatas, true),
                    Leafs = _leafSpawner.GenerateLeafStates(true),
                    SegmentLeafCounts = _leafSpawner.GetSectorLeafCounts(
                        _gameController.GetCircleDivider().Segments, 
                        _gameController.GetCircleDivider().GetAngleOfFirstDivider())
                };
                ArraySegment<byte> bytes = Writer.SerializeToByteSegment(peerStates);
                _webServer.SendAll(_connectedIds, bytes);
            }
        }
    }

    private Dictionary<int, PeerState> GeneratePeerStates(Dictionary<int, ServerPeerData> peerDatas, bool onlySendDirty)
    {
        var peerStates = new Dictionary<int, PeerState>();
        foreach (var keyValue in peerDatas)
        {
            // TODO: position and animations are not sent if position doesnt change, can this be a problem for animations?
            if (keyValue.Value.PlayerMovement.DidPositionChange() || !onlySendDirty)
            {
                peerStates[keyValue.Key] = GenerateSinglePeerState(keyValue.Value);
            }
        }

        return peerStates;
    }

    private PeerState GenerateSinglePeerState(ServerPeerData data)
    {
        PeerState peerState = new PeerState()
        {
            position = data.PlayerTransform.position,
            currentAnimation = data.AnimationController.CurrentAnimationState,
            spriteFlipped = data.AnimationController.SpriteFlipped,
            isPlaying = data.IsPlaying
        };
        return peerState;
    }

    private void OnDestroy()
    {
        _webServer.Stop();
    }
}
