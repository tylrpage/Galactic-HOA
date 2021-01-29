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
    public event Action Connected;
    public event Action Disconnected;
    
    private GameController _gameController;
    private SimpleWebClient _ws;
    private int _myId;
    private Transform _myPlayerTransform;
    private float _timer;
    private bool _connected;
    private Inputs _polledInputs;
    public Dictionary<int, ClientPeerData> _peerDatas;
    private Dictionary<int, PeerState> _peerStates;
    private Dictionary<int, LeafState> _leafStates;
    private Dictionary<int, LeafInterp> _leafInterps;
    private Camera _camera;
    public LeafSpawner _leafSpawner;
    private StateMachine _stateMachine;
    private ClientInitialData _clientInitialData;

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
        _ws.onDisconnect += WsOnonDisconnect;
        _ws.onConnect += WsOnonConnect;
        _ws.onError += delegate(Exception exception)
        {
            Debug.Log("Error: " + exception.Message);
        };
    }

    private void WsOnonDisconnect()
    {
        Disconnected?.Invoke();
        if (_connected)
            _stateMachine.StatusTextController.SetDisconnected();
        else
            _stateMachine.StatusTextController.SetServerCouldNotBeReached();
    }

    private void WsOnonConnect()
    {
        var bytes = Writer.SerializeToByteSegment(_clientInitialData);
        _ws.Send(bytes);
        
        _stateMachine.Init(this);
        Connected?.Invoke();
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
                    CreateAndRegisterPlayer(keyValue.Key, keyValue.Value, initialState.GameStateId);
                }

                foreach (var keyValue in initialState.LeafStates)
                {
                    LeafState leafState = keyValue.Value;
                    GameObject newLeaf = _leafSpawner.SpawnLeaf(keyValue.Key, leafState.position, leafState.heightInAir, leafState.rotation);
                    _leafInterps[keyValue.Key] = newLeaf.GetComponent<LeafInterp>();
                }
                
                // setup chat box
                _gameController.ChatController.ShowChat();
                _gameController.ChatController.PollForEnter = true;
                _gameController.ChatController.MessageEntered += ChatControllerOnMessageEntered;

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
                    peerData.AnimationController.SetFace(keyValue.Value.pressingSpace);
                    peerData.AnimationController.SetSpriteDirection(keyValue.Value.spriteFlipped);
                    peerData.IsPlaying = keyValue.Value.isPlaying;
                    peerData.LeafBlower.SetInputs(keyValue.Value.pressingSpace, keyValue.Value.mouseDir);
                    _gameController.ScoreController.SetPlayerScore(keyValue.Key, keyValue.Value.score);
                    peerData.PlayerSounds.SetAnimation(keyValue.Value.pressingSpace, keyValue.Value.currentAnimation);
                }

                foreach (var keyValue in peerStates.Leafs)
                {
                    LeafState leafState = keyValue.Value;
                    
                    if (!_leafInterps.ContainsKey(keyValue.Key))
                    {
                        if (leafState.IsNew)
                        {
                            GameObject newLeaf = _leafSpawner.SpawnLeaf(keyValue.Key, leafState.position, leafState.heightInAir, leafState.rotation);
                            _leafInterps[keyValue.Key] = newLeaf.GetComponent<LeafInterp>();
                        }
                    }
                    else
                    {
                        _leafInterps[keyValue.Key].PushNewTo(leafState.position, leafState.heightInAir);
                        _leafInterps[keyValue.Key].SetRotation(leafState.rotation);
                    }
                }

                break;
            }
            case 4:
            {
                NewPlayer newPlayer = new NewPlayer();
                newPlayer.Deserialize(ref bitBuffer);
                
                CreateAndRegisterPlayer(newPlayer.TheirId, newPlayer.State, _stateMachine.GetCurrentStateId());
                
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
                
                _gameController.ScoreController.RemovePlayer(playerDisconnected.TheirId);

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
            case 8:
            {
                ClearLeafs clearLeafs = new ClearLeafs();
                clearLeafs.Deserialize(ref bitBuffer);
                DestroyAllLeafs();
                
                break;
            }
            case 11:
            {
                NewChatMessage newChatMessage = new NewChatMessage();
                newChatMessage.Deserialize(ref bitBuffer);
                Debug.Log("Client received message" + newChatMessage.message);
                _gameController.ChatController.PushNewMessage(_peerDatas[newChatMessage.senderId].DisplayName, newChatMessage.message);

                break;
            }
        }
    }

    private void ChatControllerOnMessageEntered(string message)
    {
        NewChatMessage newChatMessage = new NewChatMessage()
        {
            message = message,
            senderId = _myId
        };
        var bytes = Writer.SerializeToByteSegment(newChatMessage);
        _ws.Send(bytes);
    }

    private void CreateAndRegisterPlayer(int peerId, PeerState peerState, short gameStateId)
    {
        GameObject newPlayer = Instantiate(_gameController.GetPlayerPrefab(), peerState.position, Quaternion.identity);
                    
        PositionInterp positionInterp = newPlayer.GetComponent<PositionInterp>();
        positionInterp.enabled = true;
        positionInterp.SetPosition(peerState.position);
        
        NametagController nametagController = newPlayer.GetComponent<NametagController>();
        nametagController.SetName(peerState.displayName);

        if (peerState.isColorsDirty)
        {
            PlayerColorController colorController = newPlayer.GetComponent<PlayerColorController>();
            colorController.SetPlayerColors(peerState.headColorCode, peerState.bodyColorCode, peerState.feetColorCode);
        }
                    
        _peerDatas[peerId] = new ClientPeerData()
        {
            Id = peerId,
            PositionInterp = positionInterp,
            AnimationController = newPlayer.GetComponentInChildren<AnimationController>(),
            PlayerTransform = newPlayer.transform,
            IsPlaying = peerState.isPlaying,
            LeafBlower = newPlayer.GetComponent<LeafBlower>(),
            DisplayName = peerState.displayName,
            NametagController = nametagController,
            PlayerSounds = newPlayer.GetComponent<PlayerSounds>()
        };
        
        _gameController.ScoreController.AddPlayer(peerId, peerState.displayName);
        _gameController.ScoreController.SetPlayerScore(peerId, peerState.score);

        if (_myId == peerId)
        {
            _myPlayerTransform = newPlayer.transform;
            _stateMachine.SetMyClientJoiningState(gameStateId, newPlayer.transform);
        }
        else
        {
            _stateMachine.SetOtherClientsJoiningState(gameStateId, newPlayer.transform);
        }
    }

    private void DestroyAllLeafs()
    {
        foreach (var leafInterp in _leafInterps)
        {
            Destroy(leafInterp.Value.gameObject);
        }
        _leafInterps.Clear();
    }

    public void Connect(bool isRemote, string displayName)
    {
        UriBuilder builder;
        
        _clientInitialData = new ClientInitialData()
        {
            DisplayName = displayName
        };
        
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

            // TODO: Maybe Tell character controller 2D to do client predicted movement
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

        polledInputs.MouseDir = ScreenToPlane() - (Vector2)_myPlayerTransform.position;
    }

    private Vector2 ScreenToPlane()
    {
        Vector2 viewport = _camera.ScreenToViewportPoint(Input.mousePosition);
        return new Vector2((viewport.x - 0.5f) * _camera.orthographicSize * _camera.aspect * 2, (viewport.y - 0.5f) * _camera.orthographicSize * 2);
    }
}
