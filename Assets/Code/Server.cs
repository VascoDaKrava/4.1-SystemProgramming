using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace SystemProgramming.Lesson3LLAPI
{
    [Serializable]
    public struct ConnectionPoint
    {
        public int HostID;
        public int ConnectionID;
        public int ChannelID;
        public string UserName;

        public override string ToString()
        {
            return $"Host {HostID}. Connection {ConnectionID}. Channel {ChannelID}.";
        }

        public void Clear()
        {
            HostID = 0;
            ConnectionID = 0;
            ChannelID = 0;
            UserName = "";
        }

        //public int CompareTo(object obj)
        //{
        //    throw new NotImplementedException();
        //}
    }

    [Obsolete]
    public sealed class Server : MonoBehaviour
    {
        public event Action<bool> OnServerChangeState;
        public event Action<string> OnServerConsoleNewData;
        public event Action<string> OnServerData;

        private const int MAX_CONNECTION = 10;

        [SerializeField] private string _serverIP = "192.168.31.98";
        [SerializeField] private int _serverPort = 5805;

        [Space]
        [SerializeField] private int _serverHostID;// Socket?
        [SerializeField] private int _serverChannel;

        [Space]
        [SerializeField] private bool _isStarted = false;
        [SerializeField] private byte _error;

        [Space]
        [SerializeField]
        private List<ConnectionPoint> _connections = new();

        private ConnectionPoint _sourcePoint = new();

        private void Update()
        {
            if (!_isStarted)
            {
                return;
            }

            _sourcePoint.Clear();

            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int dataSize;

            NetworkEventType networkEvent = NetworkTransport.Receive(
                out _sourcePoint.HostID,
                out _sourcePoint.ConnectionID,
                out _sourcePoint.ChannelID,
                buffer,
                bufferSize,
                out dataSize,
                out _error);

            string message = Encoding.Unicode.GetString(buffer, 0, dataSize);

            while (networkEvent != NetworkEventType.Nothing)
            {
                switch (networkEvent)
                {
                    case NetworkEventType.Nothing:
                        break;

                    case NetworkEventType.ConnectEvent:
                        Debug.Log($"S. Recieve ConnectEvent from {_sourcePoint}.");
                        OnServerConsoleNewData.Invoke($"Recieve ConnectEvent from {_sourcePoint}.");

                        if (_sourcePoint.HostID == _serverHostID)
                        {
                            Debug.LogWarning("S. This is our message? Do nothing.");
                            OnServerConsoleNewData.Invoke("This is our message? Do nothing.");
                            //break;
                        }

                        if (_connections.Contains(_sourcePoint))
                        {
                            Debug.LogWarning("S. User connection exist!");
                            OnServerConsoleNewData.Invoke("User connection exist!");
                        }
                        else
                        {
                            Debug.LogWarning("S. Point {_sourcePoint} was added.");
                            _connections.Add(_sourcePoint);
                            OnServerConsoleNewData.Invoke($"Point {_sourcePoint} was added.");
                            //SendMessageToAllPoints($"User from {_sourcePoint} has connected.");
                        }
                        break;

                    case NetworkEventType.DataEvent:
                        //SendMessageToAllPoints($"User from {sourceHostID}: {message}");
                        Debug.Log($"S. DataEvent. From {_sourcePoint}: {message}");
                        OnServerConsoleNewData.Invoke($"DataEvent. From {_sourcePoint}: {message}");
                        break;

                    case NetworkEventType.DisconnectEvent:
                        _connections.Remove(_sourcePoint);
                        OnServerConsoleNewData.Invoke($"Point {_sourcePoint} was removed.");
                        //SendMessageToAll($"Player {connectionID} has disconnected.");
                        Debug.Log($"S. User {_sourcePoint} has disconnected.");
                        OnServerConsoleNewData.Invoke($"User {_sourcePoint} has disconnected.");
                        break;

                    case NetworkEventType.BroadcastEvent:
                        break;
                }

                networkEvent = NetworkTransport.Receive(
                    out _sourcePoint.HostID,
                    out _sourcePoint.ConnectionID,
                    out _sourcePoint.ChannelID,
                    buffer,
                    bufferSize,
                    out dataSize,
                    out _error);
            }
        }

        private void OnDestroy()
        {
            StopServer();
        }

        public void StartServer()
        {
            ConnectionConfig cc = new ConnectionConfig();
            _serverChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            NetworkTransport.Init();

            _serverHostID = NetworkTransport.AddHost(topology, _serverPort, _serverIP);

            // Проверка подключения на себя
            // var serverConnectionID = NetworkTransport.Connect(_serverSocket, _serverIP, _serverPort, 0, out _error);

            if ((NetworkError)_error == NetworkError.Ok)
            {
                _isStarted = true;
                OnServerChangeState.Invoke(_isStarted);
                OnServerData.Invoke("");
                OnServerData.Invoke($"Active : \t{_isStarted}");
                OnServerData.Invoke($"Channel : \t{_serverChannel}");
                OnServerData.Invoke($"Host : \t{_serverHostID}");
                OnServerData.Invoke($"Port : \t\t{_serverPort}");
                OnServerData.Invoke($"IP : \t\t{_serverIP}");
            }
            else
            {
                Debug.LogError($"S. {(NetworkError)_error}");
                OnServerConsoleNewData.Invoke($"S. {(NetworkError)_error}");
            }
        }

        public void StopServer()
        {
            if (!_isStarted)
            {
                return;
            }

            NetworkTransport.RemoveHost(_serverHostID);
            NetworkTransport.Shutdown();
            _isStarted = false;
            OnServerChangeState.Invoke(_isStarted);
            OnServerData.Invoke("");
            OnServerData.Invoke($"Active : \t{_isStarted}");
        }

        private void SendMessageToAllPoints(string message)
        {
            foreach (var item in _connections)
            {
                ServerSendMessage(message, item);
            }
        }

        private void ServerSendMessage(string message, ConnectionPoint connectionPoint)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            //NetworkTransport.Send(_serverSocket, connectionPoint.ConnectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);
            Debug.Log($"S. Send \"{message}\" to {connectionPoint}.");
            OnServerConsoleNewData.Invoke($"S. Send \"{message}\" to {connectionPoint}.");
            NetworkTransport.Send(connectionPoint.HostID, connectionPoint.ConnectionID, connectionPoint.ChannelID, buffer, message.Length * sizeof(char), out _error);

            if ((NetworkError)_error != NetworkError.Ok)
            {
                Debug.LogError($"S. {(NetworkError)_error}");
                OnServerConsoleNewData.Invoke($"S. {(NetworkError)_error}");
            }
        }
    }
}