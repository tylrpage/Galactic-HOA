using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Messages;
using UnityEngine;
using Mirror.SimpleWeb;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    private SimpleWebClient _ws;
    private float _myId;
    private float _timer;
    private bool _connected;
    private Inputs _polledInputs;

    void Start()
    {
        TcpConfig tcpConfig = new TcpConfig(true, 5000, 20000);
        _ws = SimpleWebClient.Create(16*1024, 1000, tcpConfig);
        
        
        _ws.onConnect += delegate
        {
            Debug.Log("Client connected");
        };
        _ws.onData += WsOnonData;
        _ws.onError += delegate(Exception exception)
        {
            Debug.Log("Error: " + exception.Message);
        };
        
        Connect(true);
    }

    private void WsOnonData(ArraySegment<byte> obj)
    {
        // do stuff
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
        // GUARD
        if (!_connected)
            return;
        
        PollInputs(ref _polledInputs);
        
        _ws.ProcessMessageQueue(this);
        
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
        if (Input.GetKeyDown(KeyCode.Space))
            polledInputs.Space = true;
    }
}
