using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Messages;
using UnityEngine;
using Mirror.SimpleWeb;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class Client : MonoBehaviour
{
    private GameController _gameController;
    private SimpleWebClient _ws;
    private float _myId;
    private float _timer;
    private bool _connected;
    private Inputs _polledInputs;
    public Dictionary<int, ClientPeerData> _peerDatas;
    private Dictionary<int, PeerState> _peerStates;
    private Dictionary<int, LeafState> _leafStates;
    private Dictionary<int, LeafInterp> _leafInterps;
    private Camera _camera;
    private LeafSpawner _leafSpawner;
    private StateMachine _stateMachine;

    void Awake()
    {
        _gameController = GetComponent<GameController>();
        _camera = Camera.main;
        _leafSpawner = GetComponent<LeafSpawner>();
        _leafInterps = new Dictionary<int, LeafInterp>();
        _stateMachine = GetComponent<StateMachine>();
        
        TcpConfig tcpConfig = new TcpConfig(true, 5000, 20000);
        _ws = SimpleWebClient.Create(16*1024, 1000, tcpConfig);
        
        _ws.onData += WsOnonData;
        _ws.onConnect += WsOnonConnect;
        _ws.onError += delegate(Exception exception)
        {
            Debug.Log("Error: " + exception.Message);
        };
    }

    private void WsOnonConnect()
    {
        _stateMachine.Init(this);
    }

    private void WsOnonData(ArraySegment<byte> data)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        bitBuffer.FromArray(data.Array, data.Count);
        ushort messageId = bitBuffer.PeekUShort();
        
        //Debug.Log("Received message with id " + messageId);

        // GUARD, don't process any messages besides the initial handshake until handshake is complete
        if (!_connected && messageId != 1)
            return;

        switch (messageId)
        {
            case 1:
            {
                _peerDatas = new Dictionary<int, ClientPeerData>();
                _peerStates = new Dictionary<int, PeerState>();
                _leafStates = new Dictionary<int, LeafState>();
                
                InitialState initialState = new InitialState()
                {
                    States = _peerStates,
                    LeafStates = _leafStates
                };
                initialState.Deserialize(ref bitBuffer);
                _myId = initialState.YourId;

                foreach (var keyValue in initialState.States)
                {
                    CreateAndRegisterPlayer(keyValue.Key, keyValue.Value);
                }

                foreach (var keyValue in initialState.LeafStates)
                {
                    LeafState leafState = keyValue.Value;
                    GameObject newLeaf = _leafSpawner.SpawnLeaf(leafState.position, leafState.rotation);
                    _leafInterps[keyValue.Key] = newLeaf.GetComponent<LeafInterp>();
                }

                Debug.Log("Client connected");
                _connected = true;

                break;
            }
            case 3:
            {
                PeerStates peerStates = new PeerStates()
                {
                    States = _peerStates,
                    Leafs = _leafStates
                };
                peerStates.Deserialize(ref bitBuffer);
                
                // Translate deserialized to peer data
                foreach (var keyValue in peerStates.States)
                {
                    ClientPeerData peerData = _peerDatas[keyValue.Key];
                    peerData.PositionInterp.PushNewTo(keyValue.Value.position);
                    peerData.AnimationController.ChangeAnimationState(keyValue.Value.currentAnimation);
                    peerData.AnimationController.SetSpriteDirection(keyValue.Value.spriteFlipped);
                    peerData.IsPlaying = keyValue.Value.isPlaying;
                }

                foreach (var keyValue in peerStates.Leafs)
                {
                    LeafState leafState = keyValue.Value;
                    _leafInterps[keyValue.Key].PushNewTo(leafState.position, leafState.heightInAir);
                    _leafInterps[keyValue.Key].SetRotation(leafState.rotation);
                }

                break;
            }
            case 4:
            {
                NewPlayer newPlayer = new NewPlayer();
                newPlayer.Deserialize(ref bitBuffer);
                
                CreateAndRegisterPlayer(newPlayer.TheirId, newPlayer.State);
                
                break;
            }
            case 5:
            {
                PlayerDisconnected playerDisconnected = new PlayerDisconnected();
                playerDisconnected.Deserialize(ref bitBuffer);

                int id = playerDisconnected.TheirId;
                GameObject playerObjectToDestroy = _peerDatas[id].PositionInterp.gameObject;
                Destroy(playerObjectToDestroy);
                _peerDatas.Remove(id);
                _peerStates.Remove(id);

                break;
            }
            case 6:
            {
                ZoneCountChange zoneCountChange = new ZoneCountChange();
                zoneCountChange.Deserialize(ref bitBuffer);
                _gameController.GetCircleDivider().SetSegments(zoneCountChange.NewZoneCount);
                _gameController.GetCircleDivider().SetArrowsSegment(zoneCountChange.YourSegment);
                _gameController.GetCircleDivider().CalcAndSetArrowTarget(zoneCountChange.NewZoneCount);
                
                break;
            }
            case 7:
            {
                StateChange stateChange = new StateChange();
                stateChange.Deserialize(ref bitBuffer);
                _stateMachine.SetStateId(stateChange.StateId);

                break;
            }
        }
    }

    private void CreateAndRegisterPlayer(int peerId, PeerState peerState)
    {
        GameObject newPlayer = Instantiate(_gameController.GetPlayerPrefab(), peerState.position, Quaternion.identity);
                    
        PositionInterp positionInterp = newPlayer.GetComponent<PositionInterp>();
        positionInterp.enabled = true;
        positionInterp.SetPosition(peerState.position);
                    
        _peerDatas[peerId] = new ClientPeerData()
        {
            Id = peerId,
            PositionInterp = positionInterp,
            AnimationController = newPlayer.GetComponentInChildren<AnimationController>(),
            PlayerTransform = newPlayer.transform,
            IsPlaying = peerState.isPlaying
        };
    }

    public void Connect(bool isRemote)
    {
        UriBuilder builder;
        
        if (isRemote)
        {
            builder = new UriBuilder()
            {
                Scheme = "wss",
                Host = "tylrpage.com",
                Port = Constants.GAME_PORT
            };
        }
        else
        {
            builder = new UriBuilder()
            {
                Scheme = "ws",
                Host = "localhost",
                Port = Constants.GAME_PORT
            };
        }
        Debug.Log("Connecting to " + builder.Uri);
        _ws.Connect(builder.Uri);
    }

    private void OnDestroy()
    {
        if (_ws != null)
        {
            _ws.Disconnect();
        }
    }

    private void Update()
    {
        _ws.ProcessMessageQueue(this);
        
        // GUARD
        if (!_connected)
            return;
        
        PollInputs(ref _polledInputs);

        _timer += Time.deltaTime;
        while (_timer >= Constants.STEP)
        {
            _timer -= Constants.STEP;

            // Tell the server my inputs
            ClientInputs clientInputs = new ClientInputs()
            {
                inputs = _polledInputs
            };
            ArraySegment<byte> bytes = Writer.SerializeToByteSegment(clientInputs);
            _ws.Send(bytes);

            _polledInputs = Inputs.EmptyInputs();

            // Maybe Tell character controller 2D to do client predicted movement
        }
    }

    private void PollInputs(ref Inputs polledInputs)
    {
        if (Input.GetKey(KeyCode.W))
            polledInputs.W = true;
        if (Input.GetKey(KeyCode.A))
            polledInputs.A = true;
        if (Input.GetKey(KeyCode.S))
            polledInputs.S = true;
        if (Input.GetKey(KeyCode.D))
            polledInputs.D = true;
        if (Input.GetKey(KeyCode.Space))
            polledInputs.Space = true;

        polledInputs.MouseDir = _camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
    }
}
