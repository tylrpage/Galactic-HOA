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
    private GameObject _playerPrefab;
    private SimpleWebServer _webServer;
    private Dictionary<int, ServerPeerData> _peerDatas;
    private Dictionary<int, PeerState> _peerStates;
    private List<int> _connectedIds;
    private bool _listening;
    private float _timer;

    private void Awake()
    {
        _peerDatas = new Dictionary<int, ServerPeerData>();
        _playerPrefab = GetComponent<GameController>().GetPlayerPrefab();
        _connectedIds = new List<int>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _webServer = Listen();

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

    private void WebServerOnonConnect(int peerId)
    {
        GameObject newPlayer = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
        
        Movement movement = newPlayer.GetComponent<Movement>();
        movement.enabled = true;
        LeafBlower leafBlower = newPlayer.GetComponent<LeafBlower>();
        leafBlower.enabled = true;
        newPlayer.GetComponent<CircleCollider2D>().enabled = true;

        ServerPeerData peerData = new ServerPeerData()
        {
            Id = peerId,
            Inputs = Inputs.EmptyInputs(),
            PlayerMovement = movement,
            PlayerTransform = newPlayer.transform,
            PlayerBlower = leafBlower
        };
        _peerDatas[peerId] = peerData;
        _connectedIds.Add(peerId);
        
        // Give connecting peer his initial handshake data
        InitialState initialState = new InitialState()
        {
            States = GeneratePeerStates(_peerDatas),
            YourId = peerId
        };
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(initialState);
        _webServer.SendOne(peerId, bytes);
    }

    private void WebServerOnonDisconnect(int id)
    {
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
            
            // send states
            PeerStates peerStates = new PeerStates()
            {
                States = GeneratePeerStates(_peerDatas)
            };
            ArraySegment<byte> bytes = Writer.SerializeToByteSegment(peerStates);
            _webServer.SendAll(_connectedIds, bytes);
        }
    }

    private Dictionary<int, PeerState> GeneratePeerStates(Dictionary<int, ServerPeerData>  peerDatas)
    {
        var peerStates = new Dictionary<int, PeerState>();
        foreach (var keyValue in peerDatas)
        {
            peerStates[keyValue.Key] = new PeerState()
            {
                position = keyValue.Value.PlayerTransform.position
            };
        }

        return peerStates;
    } 

    private void OnDestroy()
    {
        _webServer.Stop();
    }
}
