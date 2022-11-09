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

        [SerializeField] private int _hostID;
        [SerializeField] private int _port = 0;// 0 = random port
        [SerializeField] private int _reliableChannel;

        [Space]
        [SerializeField] private int _connectionID;

        [Space]
        [SerializeField] private string _serverIP = "192.168.31.98";
        [SerializeField] private int _serverPort = 5805;

        [Space]
        [SerializeField] private bool _isConnected = false;
        [SerializeField] private byte _error;

        public void Connect()
        {
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();
            _reliableChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            _hostID = NetworkTransport.AddHost(topology, _port);
            _connectionID = NetworkTransport.Connect(_hostID, _serverIP, _serverPort, 0, out _error);

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

        public void Disconnect()
        {
            if (!_isConnected)
            {
                return;
            }

            NetworkTransport.Disconnect(_hostID, _connectionID, out _error);
            _isConnected = false;
            OnClientChangeState.Invoke(_isConnected);
        }

        private void Update()
        {
            if (!_isConnected)
            {
                return;
            }

            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;

            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);

            while (recData != NetworkEventType.Nothing)
            {
                switch (recData)
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

                recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
            }
        }

        public new void SendMessage(string message)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(_hostID, _connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);
            
            if ((NetworkError)_error != NetworkError.Ok)
            {
                Debug.LogError((NetworkError)_error);
            }
        }
    }
}