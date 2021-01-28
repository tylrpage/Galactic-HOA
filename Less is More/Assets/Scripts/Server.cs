using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private HashSet<int> _unHandShakenPeers;
    private Dictionary<int, PeerState> _peerStates;
    private List<int> _connectedIds;
    private bool _listening;
    private float _timer;
    public LeafSpawner _leafSpawner;
    private GameController _gameController;
    private StateMachine _stateMachine;

    private void Awake()
    {
        _peerDatas = new Dictionary<int, ServerPeerData>();
        _gameController = GetComponent<GameController>();
        _connectedIds = new List<int>();
        _leafSpawner = GetComponent<LeafSpawner>();
        _stateMachine = GetComponent<StateMachine>();
        _unHandShakenPeers = new HashSet<int>();
    }

    // Start is called before the first frame update
    void Start()
    {
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

    public void ScoreLeafCounts(List<ushort> leafCounts)
    {
        List<ServerPeerData> playingPeers = _peerDatas.Values.Where(x => x.IsPlaying).ToList();

        if (playingPeers.Count > 0)
        {
            for (int i = 0; i < leafCounts.Count; i++)
            {
                playingPeers[i].Score += (ushort)(leafCounts[i] * Constants.FINE_PER_LEAF);
            }
        }
    }

    public void NotifyClientsToClearLeafs()
    {
        ClearLeafs clearLeafs = new ClearLeafs();
        var bytes = Writer.SerializeToByteSegment(clearLeafs);
        _webServer.SendAll(_connectedIds, bytes);
    }

    public void NotifyClientOfZoneCount(int peerId, ZoneCountChange zoneCountChange)
    {
        var bytes = Writer.SerializeToByteSegment(zoneCountChange);
        _webServer.SendOne(peerId, bytes);
    }

    private void WebServerOnonConnect(int peerId)
    {
        _unHandShakenPeers.Add(peerId);
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

                Inputs inputsToUse;
                if (_stateMachine.ShouldLockNonPlayingPlayers() && !_peerDatas[peerId].IsPlaying)
                    inputsToUse = Inputs.EmptyInputs();
                else
                    inputsToUse = clientInputs.inputs;
                _peerDatas[peerId].Inputs = inputsToUse;
                _peerDatas[peerId].PlayerMovement.SetInputs(inputsToUse);
                _peerDatas[peerId].PlayerBlower.SetInputs(inputsToUse.Space, inputsToUse.MouseDir);

                break;
            }
            case 9:
            {
                ClientInitialData clientInitialData = new ClientInitialData();
                clientInitialData.Deserialize(ref bitBuffer);

                _unHandShakenPeers.Remove(peerId);
                
                Vector3 spawnPosition = _gameController.SpawnPoint.position + _stateMachine.GroundControl.GetGroundOffset();

                GameObject newPlayer = Instantiate(_gameController.GetPlayerPrefab(), spawnPosition, Quaternion.identity);

                _stateMachine.SetServerJoiningState(newPlayer.transform);
        
                Movement movement = newPlayer.GetComponent<Movement>();
                movement.enabled = true;
                LeafBlower leafBlower = newPlayer.GetComponent<LeafBlower>();
                leafBlower.enabled = true;
                newPlayer.GetComponent<CircleCollider2D>().enabled = true;
                newPlayer.GetComponent<LeafBlower>().Simulate = true;

                ServerPeerData newPeerData = new ServerPeerData()
                {
                    Id = peerId,
                    Inputs = Inputs.EmptyInputs(),
                    PlayerMovement = movement,
                    PlayerTransform = newPlayer.transform,
                    PlayerBlower = leafBlower,
                    AnimationController = newPlayer.GetComponentInChildren<AnimationController>(),
                    displayName = clientInitialData.DisplayName
                };
                _peerDatas[peerId] = newPeerData;
        
                // Send everyone a message about this player
                NewPlayer newPlayerMessage = new NewPlayer()
                {
                    TheirId = peerId,
                    State = GenerateSinglePeerState(newPeerData, true),
                };
                ArraySegment<byte> newPlayerMessageBytes = Writer.SerializeToByteSegment(newPlayerMessage);
                _webServer.SendAll(_connectedIds, newPlayerMessageBytes);
        
                // Give connecting peer his initial handshake data
                _connectedIds.Add(peerId);
                InitialState initialState = new InitialState()
                {
                    States = GeneratePeerStates(_peerDatas, false),
                    LeafStates = _leafSpawner.GenerateLeafStates(false),
                    YourId = peerId,
                    GameStateId = _stateMachine.GetCurrentStateId()
                };
                ArraySegment<byte> bytes = Writer.SerializeToByteSegment(initialState);
                _webServer.SendOne(peerId, bytes);

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
            if (keyValue.Value.PlayerMovement.DidInputsChange(keyValue.Value.Inputs) || !onlySendDirty)
            {
                peerStates[keyValue.Key] = GenerateSinglePeerState(keyValue.Value, !onlySendDirty);
            }
        }

        return peerStates;
    }

    private PeerState GenerateSinglePeerState(ServerPeerData data, bool includeDisplayName)
    {
        PeerState peerState = new PeerState()
        {
            position = data.PlayerTransform.position,
            currentAnimation = data.AnimationController.CurrentAnimationState,
            spriteFlipped = data.AnimationController.SpriteFlipped,
            isPlaying = data.IsPlaying,
            pressingSpace = data.Inputs.Space,
            mouseDir = data.Inputs.MouseDir,
            score = data.Score
        };
        peerState.displayName = includeDisplayName ? data.displayName : "";
        
        return peerState;
    }

    private void OnDestroy()
    {
        _webServer.Stop();
    }
}
