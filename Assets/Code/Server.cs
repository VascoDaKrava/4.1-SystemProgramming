using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace SystemProgramming.Lesson3LLAPI
{
    [Obsolete]
    public sealed class Server : MonoBehaviour
    {
        public event Action<bool> OnServerChangeState;

        private const int MAX_CONNECTION = 10;

        [SerializeField] private string _serverIP = "192.168.31.98";
        [SerializeField] private int _serverPort = 5805;
        [SerializeField] private int _serverSocket;
        [SerializeField] private int _reliableChannel;

        [Space]
        [SerializeField] private int _connectionID;

        [Space]
        [SerializeField] private bool _isStarted = false;
        [SerializeField] private byte _error;

        [Space]
        [SerializeField] private Dictionary<int, string> _connectionIDs = new();

        private void Update()
        {
            if (!_isStarted)
            {
                return;
            }

            int sourceHostID;
            int sourceConnectionID;
            int sourceChannelID;
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int dataSize;
            //int stopFactor = 5;

            NetworkEventType networkEvent = NetworkTransport.Receive(out sourceHostID, out sourceConnectionID, out sourceChannelID, buffer, bufferSize, out dataSize, out _error);
            string message = Encoding.Unicode.GetString(buffer, 0, dataSize);

            while (networkEvent != NetworkEventType.Nothing)
            {
                //if (sourceHostID == _hostID)
                //{
                //    Debug.Log($"Message from Host {sourceHostID} / Con {connectionID} / Ch {channelID}");
                //    stopFactor--;
                //    if (stopFactor == 0)
                //    {
                //        Debug.LogWarning("LOOP");
                //        break;
                //    }
                //    continue;
                //}

                switch (networkEvent)
                {
                    case NetworkEventType.Nothing:
                        break;

                    case NetworkEventType.ConnectEvent:
                        if (_connectionIDs.TryAdd(sourceConnectionID, "qwe"))
                        {
                            //SendMessageToAll($"Player {connectionID} has connected.");
                            Debug.Log($"Recieve ConnectEvent from {sourceHostID} / {sourceConnectionID} / {sourceChannelID}.");
                        }
                        else
                        {
                            Debug.Log($"Recieve ConnectEvent from {sourceHostID} / {sourceConnectionID} / {sourceChannelID}.");
                            Debug.LogWarning($"User exist with same ConnectionID ({sourceConnectionID})");
                        }
                        break;

                    case NetworkEventType.DataEvent:
                        //SendMessageToAll($"Player {connectionID}: {message}");
                        Debug.Log($"Player {sourceConnectionID}: {message}");
                        break;

                    case NetworkEventType.DisconnectEvent:
                        _connectionIDs.Remove(sourceConnectionID);
                        //SendMessageToAll($"Player {connectionID} has disconnected.");
                        Debug.Log($"Player {sourceConnectionID} has disconnected.");
                        break;

                    case NetworkEventType.BroadcastEvent:
                        break;
                }

                networkEvent = NetworkTransport.Receive(out sourceHostID, out sourceConnectionID, out sourceChannelID, buffer, bufferSize, out dataSize, out _error);
            }
        }

        private void OnDestroy()
        {
            StopServer();
        }

        public void StartServer()
        {
            ConnectionConfig cc = new ConnectionConfig();
            _reliableChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            NetworkTransport.Init();

            _serverSocket = NetworkTransport.AddHost(topology, _serverPort);
            // Для проверки соединения и иное, можно создать подключение на себя
            //_connectionID = NetworkTransport.Connect(_serverSocket, _serverIP, _serverPort, 0, out _error);

            if ((NetworkError)_error == NetworkError.Ok)
            {
                _isStarted = true;
                OnServerChangeState.Invoke(_isStarted);
            }
            else
            {
                Debug.LogError((NetworkError)_error);
            }
        }

        public void StopServer()
        {
            if (!_isStarted)
            {
                return;
            }

            NetworkTransport.RemoveHost(_serverSocket);
            NetworkTransport.Shutdown();
            _isStarted = false;
            OnServerChangeState.Invoke(_isStarted);
        }

        private void SendMessageToAll(string message)
        {
            foreach (var item in _connectionIDs.Keys)
            {
                ServerSendMessage(message, item);
            }
        }

        private void ServerSendMessage(string message, int connectionID)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(_serverSocket, connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);

            if ((NetworkError)_error != NetworkError.Ok)
            {
                Debug.Log((NetworkError)_error);
            }
        }
    }
}