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

        [SerializeField] private int _hostID;
        [SerializeField] private int _port = 5805;
        [SerializeField] private int _reliableChannel;

        [Space]
        [SerializeField] private bool _isStarted = false;
        [SerializeField] private byte _error;

        [Space]
        [SerializeField] private List<int> _connectionIDs = new();

        public void StartServer()
        {
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();
            _reliableChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            _hostID = NetworkTransport.AddHost(topology, _port);
            _isStarted = true;
            OnServerChangeState.Invoke(_isStarted);
        }

        public void ShutDownServer()
        {
            if (!_isStarted)
            {
                return;
            }

            NetworkTransport.RemoveHost(_hostID);
            NetworkTransport.Shutdown();
            _isStarted = false;
            OnServerChangeState.Invoke(_isStarted);
        }

        private void Update()
        {
            if (!_isStarted)
            {
                return;
            }

            int sourceHostID;
            int connectionID;
            int channelID;
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int dataSize;
            int stopFactor = 5;

            NetworkEventType recData = NetworkTransport.Receive(out sourceHostID, out connectionID, out channelID, buffer, bufferSize, out dataSize, out _error);

            while (recData != NetworkEventType.Nothing)
            {
                if (sourceHostID == _hostID)
                {
                    Debug.Log($"Message from Host {sourceHostID} / Con {connectionID} / Ch {channelID}");
                    stopFactor--;
                    if (stopFactor == 0)
                    {
                        Debug.LogWarning("LOOP");
                        break;
                    }
                    continue;
                }

                switch (recData)
                {
                    case NetworkEventType.Nothing:
                        break;

                    case NetworkEventType.ConnectEvent:
                        _connectionIDs.Add(connectionID);
                        SendMessageToAll($"Player {connectionID} has connected.");
                        Debug.Log($"Player {connectionID} has connected.");
                        break;

                    case NetworkEventType.DataEvent:
                        string message = Encoding.Unicode.GetString(buffer, 0, dataSize);
                        SendMessageToAll($"Player {connectionID}: {message}");
                        Debug.Log($"Player {connectionID}: {message}");
                        break;

                    case NetworkEventType.DisconnectEvent:
                        _connectionIDs.Remove(connectionID);
                        SendMessageToAll($"Player {connectionID} has disconnected.");
                        Debug.Log($"Player {connectionID} has disconnected.");
                        break;

                    case NetworkEventType.BroadcastEvent:
                        break;
                }

                recData = NetworkTransport.Receive(out sourceHostID, out connectionID, out channelID, buffer, bufferSize, out dataSize, out _error);
            }
        }

        public void SendMessageToAll(string message)
        {
            for (int i = 0; i < _connectionIDs.Count; i++)
            {
                SendMessage(message, _connectionIDs[i]);
            }
        }

        public void SendMessage(string message, int connectionID)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(_hostID, connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);

            if ((NetworkError)_error != NetworkError.Ok)
            {
                Debug.Log((NetworkError)_error);
            }
        }
    }
}