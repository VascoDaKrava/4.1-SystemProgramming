using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace SystemProgramming.Lesson3LLAPI
{
    [Obsolete]
    public sealed class Client : MonoBehaviour
    {
        public event Action<string> OnMessageReceive;
        public event Action<bool> OnClientChangeState;

        private const int MAX_CONNECTION = 10;

        [SerializeField] private int _clientSocket;
        [SerializeField] private int _clientPort = 0;// 0 = random port
        [SerializeField] private int _reliableChannel;

        [Space]
        [SerializeField] private int _connectionID;

        [Space]
        [SerializeField] private string _serverIP = "192.168.31.98";
        [SerializeField] private int _serverPort = 5805;

        [Space]
        [SerializeField] private bool _isConnected = false;
        [SerializeField] private byte _error;

        private void Update()
        {
            if (!_isConnected)
            {
                return;
            }

            int recHostId;
            int connectionId;
            int channelId;
            int bufferSize = 1024;
            byte[] recBuffer = new byte[bufferSize];
            int dataSize;

            NetworkEventType networkEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);

            while (networkEvent != NetworkEventType.Nothing)
            {
                switch (networkEvent)
                {
                    case NetworkEventType.Nothing:
                        break;

                    case NetworkEventType.ConnectEvent:
                        OnMessageReceive?.Invoke($"You have been connected to server.");
                        Debug.LogWarning($"You have been connected to server.");
                        break;

                    case NetworkEventType.DataEvent:
                        string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                        OnMessageReceive?.Invoke(message);
                        Debug.Log(message);
                        break;

                    case NetworkEventType.DisconnectEvent:
                        _isConnected = false;
                        OnMessageReceive?.Invoke($"You have been disconnected from server.");
                        Debug.LogWarning($"You have been disconnected from server.");
                        break;

                    case NetworkEventType.BroadcastEvent:
                        break;
                }

                networkEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
            }
        }

        private void OnDestroy()
        {
            ClientDisconnect();
        }

        public void ClientConnect()
        {
            ConnectionConfig cc = new ConnectionConfig();
            _reliableChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            NetworkTransport.Init();

            _clientSocket = NetworkTransport.AddHost(topology, _clientPort);
            _connectionID = NetworkTransport.Connect(_clientSocket, _serverIP, _serverPort, 0, out _error);

            if ((NetworkError)_error == NetworkError.Ok)
            {
                _isConnected = true;
                OnClientChangeState.Invoke(_isConnected);
            }
            else
            {
                Debug.LogError((NetworkError)_error);
            }
        }

        public void ClientDisconnect()
        {
            if (!_isConnected)
            {
                return;
            }

            NetworkTransport.Disconnect(_clientSocket, _connectionID, out _error);
            _isConnected = false;
            OnClientChangeState.Invoke(_isConnected);
        }

        public void ClientSendMessage(string message)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(_clientSocket, _connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);
            
            if ((NetworkError)_error != NetworkError.Ok)
            {
                Debug.LogError((NetworkError)_error);
            }
        }
    }
}