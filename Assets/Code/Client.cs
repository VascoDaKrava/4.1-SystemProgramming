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
        public event Action<string> OnClientConsoleNewData;
        public event Action<string> OnClientData;

        private const int MAX_CONNECTION = 10;

        [SerializeField] public string ClientIP;// = "192.168.31.148";
        //[SerializeField] private string _clientIP = "192.168.31.98";
        [SerializeField] public int ClientPort;// = 20123;// 0 = random port ?

        [Space]
        [SerializeField] public string ServerIP;// = "192.168.31.98";
        [SerializeField] public int ServerPort;// = 5805;

        [Space]
        [SerializeField] private int _clientHostID;// Socket?
        [SerializeField] private int _clientConnectionID;
        [SerializeField] private int _clientChannel;

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
            string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);

            while (networkEvent != NetworkEventType.Nothing)
            {
                switch (networkEvent)
                {
                    case NetworkEventType.Nothing:
                        break;

                    case NetworkEventType.ConnectEvent:
                        OnMessageReceive?.Invoke($"You have been connected to server.");
                        Debug.LogWarning($"C. Catch ConnectEvent.");
                        OnClientConsoleNewData.Invoke($"Catch ConnectEvent.");
                        break;

                    case NetworkEventType.DataEvent:
                        OnMessageReceive?.Invoke(message);
                        Debug.Log($"C. User catch DataEvent : \"{message}\"");
                        OnClientConsoleNewData.Invoke($"Catch DataEvent : \"{message}\"");
                        break;

                    case NetworkEventType.DisconnectEvent:
                        Debug.LogWarning($"C. Catch DisconnectEvent.");
                        OnClientConsoleNewData.Invoke($"Catch DisconnectEvent.");
                        _isConnected = false;
                        OnMessageReceive?.Invoke($"You have been disconnected from server.");
                        OnClientChangeState.Invoke(_isConnected);
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
            _clientChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            NetworkTransport.Init();

            _clientHostID = NetworkTransport.AddHost(topology, ClientPort, ClientIP);
            _clientConnectionID = NetworkTransport.Connect(_clientHostID, ServerIP, ServerPort, 0, out _error);

            if ((NetworkError)_error == NetworkError.Ok)
            {
                _isConnected = true;
                OnClientChangeState.Invoke(_isConnected);
                OnClientData.Invoke($"");
                OnClientData.Invoke($"Active : \t{_isConnected}");
                OnClientData.Invoke($"Serv. Port : \t{ServerPort}");
                OnClientData.Invoke($"Serv. IP : \t{ServerIP}");
                OnClientData.Invoke($"Channel : \t{_clientChannel}");
                OnClientData.Invoke($"Connect. : \t{_clientConnectionID}");
                OnClientData.Invoke($"Host : \t{_clientHostID}");
                OnClientData.Invoke($"Port : \t\t{ClientPort}");
                OnClientData.Invoke($"IP : \t\t{ClientIP}");
            }
            else
            {
                Debug.LogError($"C. {(NetworkError)_error}");
                OnClientConsoleNewData.Invoke($"NetworkError. {(NetworkError)_error}");
            }
        }

        public void ClientDisconnect()
        {
            if (!_isConnected)
            {
                return;
            }

            NetworkTransport.Disconnect(_clientHostID, _clientConnectionID, out _error);
            _isConnected = false;
            OnClientChangeState.Invoke(_isConnected);
            OnClientData.Invoke($"");
            OnClientData.Invoke($"Active : \t{_isConnected}");
        }

        public void ClientSendMessage(string message)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(_clientHostID, _clientConnectionID, _clientChannel, buffer, message.Length * sizeof(char), out _error);
            OnClientConsoleNewData.Invoke($"Send {{{_clientHostID}, {_clientConnectionID}, {_clientChannel}}} / {message}");

            if ((NetworkError)_error != NetworkError.Ok)
            {
                Debug.LogError($"C. {(NetworkError)_error}");
                OnClientConsoleNewData.Invoke($"NetworkError. {(NetworkError)_error}");
            }
        }
    }
}