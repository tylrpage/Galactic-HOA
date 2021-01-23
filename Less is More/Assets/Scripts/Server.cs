using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using Mirror.SimpleWeb;
using NetStack.Quantization;
using NetStack.Serialization;

public class Server : MonoBehaviour
{
    private SimpleWebServer _webServer;
    private List<int> _connectedIds;
    private bool _connected;
    private float _timer;
     
    // Start is called before the first frame update
    void Start()
    {
        _connectedIds = new List<int>();

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
        _connectedIds.Add(id);
    }

    private void WebServerOnonDisconnect(int id)
    {
        _connectedIds.Remove(id);
    }

    private void WebServerOnonData(int id, ArraySegment<byte> data)
    {
        
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
