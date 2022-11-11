using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace SystemProgramming.Lesson3LLAPI
{
    [Obsolete]
    public sealed class UI : MonoBehaviour
    {
        [Space]
        [SerializeField] private Color _activeColor = Color.green;
        [SerializeField] private Color _inactiveColor = Color.red;

        [Space]
        [SerializeField] private Server _server;
        [SerializeField] private TMP_Text _titleServer;
        [SerializeField] private Button _buttonStartServer;
        [SerializeField] private Button _buttonStopServer;

        [Space]
        [SerializeField] private Client _client;
        [SerializeField] private TMP_Text _titleCliect;
        [SerializeField] private Button _buttonConnectClient;
        [SerializeField] private Button _buttonDisconnectClient;
        [SerializeField] private Button _buttonSendMessage;

        [Space]
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _textField;

        private void Start()
        {
            _textField.text = "";
            
            _buttonStartServer.onClick.AddListener(ServerStartHandler);
            _buttonStopServer.onClick.AddListener(ServerStopHandler);
            _buttonConnectClient.onClick.AddListener(ClientConnectHandler);
            _buttonDisconnectClient.onClick.AddListener(ClientDisconnectHandler);
            _buttonSendMessage.onClick.AddListener(ClientSendMessageHandler);
            
            _client.OnMessageReceive += ClientReceiveMessageHandler;
            _client.OnClientChangeState += OnClientChangeStateHandler;

            _server.OnServerChangeState += OnServerChangeStateHandler;
        }

        private void OnDestroy()
        {
            _buttonStartServer.onClick.RemoveAllListeners();
            _buttonStopServer.onClick.RemoveAllListeners();
            _buttonConnectClient.onClick.RemoveAllListeners();
            _buttonDisconnectClient.onClick.RemoveAllListeners();
            _buttonSendMessage.onClick.RemoveAllListeners();
            
            _client.OnMessageReceive -= ClientReceiveMessageHandler;
            _client.OnClientChangeState -= OnClientChangeStateHandler;
            
            _server.OnServerChangeState -= OnServerChangeStateHandler;
        }

        private void OnServerChangeStateHandler(bool state)
        {
            _buttonStartServer.interactable = !state;
            _buttonStopServer.interactable = state;
            _titleServer.color = state ? _activeColor : _inactiveColor;
        }

        private void OnClientChangeStateHandler(bool state)
        {
            _buttonConnectClient.interactable = !state;
            _buttonDisconnectClient.interactable = state;
            _titleCliect.color = state ? _activeColor : _inactiveColor;
        }

        private void ServerStartHandler()
        {
            _server.StartServer();
        }

        private void ServerStopHandler()
        {
            _server.StopServer();
        }

        private void ClientConnectHandler()
        {
            _client.ClientConnect();
        }

        private void ClientDisconnectHandler()
        {
            _client.ClientDisconnect();
        }

        private void ClientSendMessageHandler()
        {
            _client.ClientSendMessage(_inputField.text);
            _inputField.text = "";
        }

        public void ClientReceiveMessageHandler(string message)
        {
            _textField.text = $"{message}\n{_textField.text}";
        }
    }
}