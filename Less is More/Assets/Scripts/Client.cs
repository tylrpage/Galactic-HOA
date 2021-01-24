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
    private GameObject _playerPrefab;
    private SimpleWebClient _ws;
    private float _myId;
    private float _timer;
    private bool _connected;
    private Inputs _polledInputs;
    private Dictionary<int, ClientPeerData> _peerDatas;
    private Dictionary<int, PeerState> _peerStates;
    private Dictionary<int, LeafState> _leafStates;
    private Dictionary<int, PositionInterp> _leafInterps;
    private Camera _camera;
    private LeafSpawner _leafSpawner;

    void Awake()
    {
        _camera = Camera.main;
        _playerPrefab = GetComponent<GameController>().GetPlayerPrefab();
        _leafSpawner = GetComponent<LeafSpawner>();
        _leafInterps = new Dictionary<int, PositionInterp>();
        
        TcpConfig tcpConfig = new TcpConfig(true, 5000, 20000);
        _ws = SimpleWebClient.Create(16*1024, 1000, tcpConfig);
        
        _ws.onData += WsOnonData;
        _ws.onError += delegate(Exception exception)
        {
            Debug.Log("Error: " + exception.Message);
        };
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
                    PeerState peerState = keyValue.Value;
                    int peerId = keyValue.Key;
            
                    GameObject newPlayer = Instantiate(_playerPrefab, peerState.position, Quaternion.identity);
                    
                    PositionInterp positionInterp = newPlayer.GetComponent<PositionInterp>();
                    positionInterp.enabled = true;
                    Debug.Log(keyValue.Value.position);
                    positionInterp.SetPosition(keyValue.Value.position);
                    
                    _peerDatas[peerId] = new ClientPeerData()
                    {
                        Id = peerId,
                        PositionInterp = positionInterp,
                        AnimationController = newPlayer.GetComponentInChildren<AnimationController>()
                    };
                }

                foreach (var keyValue in initialState.LeafStates)
                {
                    LeafState leafState = keyValue.Value;
                    GameObject newLeaf = _leafSpawner.SpawnLeaf(leafState.position, leafState.rotation);
                    _leafInterps[keyValue.Key] = newLeaf.GetComponent<PositionInterp>();
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
                
                // Send new position to the player's interpolation controller
                foreach (var keyValue in peerStates.States)
                {
                    _peerDatas[keyValue.Key].PositionInterp.PushNewTo(keyValue.Value.position);
                    _peerDatas[keyValue.Key].AnimationController.ChangeAnimationState(keyValue.Value.currentAnimation);
                    _peerDatas[keyValue.Key].AnimationController.SetSpriteDirection(keyValue.Value.spriteFlipped);
                }

                foreach (var keyValue in peerStates.Leafs)
                {
                    LeafState leafState = keyValue.Value;
                    _leafInterps[keyValue.Key].PushNewTo(leafState.position);
                    _leafInterps[keyValue.Key].SetRotation(leafState.rotation);
                }

                break;
            }
        }
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
