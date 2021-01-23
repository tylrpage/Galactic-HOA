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
#pragma warning disable 0649
    [SerializeField] private GameObject playerPrefab;
#pragma warning restore 0649
    
    private SimpleWebServer _webServer;
    private Dictionary<int, PeerData> _peerDatas;
    private bool _connected;
    private float _timer;

    private void Awake()
    {
        _peerDatas = new Dictionary<int, PeerData>();
    }

    // Start is called before the first frame update
    void Start()
    {
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
        _webServer = new SimpleWebServer(10000, tcpConfig, 16*1024, 3000, sslConfig);
        _webServer.Start(Constants.GAME_PORT);
        
        Debug.Log("Server started");
        
        _webServer.onConnect += WebServerOnonConnect;
        
        _webServer.onData += WebServerOnonData;
        
        _webServer.onError += delegate(int i, Exception exception)
        {
            Debug.Log("Error: " + exception.Message);
        };
        
        _webServer.onDisconnect += WebServerOnonDisconnect;
    }

    private void WebServerOnonConnect(int id)
    {
        GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        PeerData peerData = new PeerData()
        {
            Id = id,
            Inputs = Inputs.EmptyInputs(),
            PlayerMovement = newPlayer.GetComponent<Movement>(),
            PlayerTransform = newPlayer.transform
        };
        _peerDatas[id] = peerData;
    }

    private void WebServerOnonDisconnect(int id)
    {
        _peerDatas.Remove(id);
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

                break;
            }
        }
    }

    void Update()
    {
        // GUARD
        if (!_connected)
            return;
        
        _timer += Time.deltaTime;
        while (_timer >= Constants.STEP)
        {
            _timer -= Constants.STEP;
            
            // send states
        }
    }

    private void OnDestroy()
    {
        _webServer.Stop();
    }
}
