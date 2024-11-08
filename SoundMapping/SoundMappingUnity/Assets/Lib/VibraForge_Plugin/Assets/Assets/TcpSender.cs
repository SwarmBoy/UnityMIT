using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class TcpSender : MonoBehaviour
{

    public String Host = "localhost";
    public Int32 Port = 9051;

    public TcpClient client = null;
    NetworkStream stream = null;
    public bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        client = new TcpClient();
        if (SetupSocket())
        {
            isConnected = true;
            Debug.Log("socket is set up");
            Debug.Log("buffer size length = "+client.SendBufferSize);
        }
        else
        {
            isConnected = false;
            Debug.Log("socket setup failed");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void SendData(String data)
    {
        if (isConnected)
        {
            Byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(data);
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
            Debug.Log("socket is sent");
        }
        else
        {
            Debug.Log("no socket is sent");
        }
    }

    public bool SetupSocket()
    {
        try
        {
            client.Connect(Host, Port);
            client.SendBufferSize = 512;
            client.ReceiveBufferSize = 512;
            stream = client.GetStream();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
            return false;
        }
    }

}
