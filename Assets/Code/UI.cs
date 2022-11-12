using System;
using TMPro;
using UnityEditor.VersionControl;
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
        [SerializeField] private TMP_Text _dataServerText;
        [SerializeField] private TMP_Text _consoleServerText;

        [Space]
        [SerializeField] private Client _client;
        [SerializeField] private TMP_Text _titleClient;
        [SerializeField] private Button _buttonConnectClient;
        [SerializeField] private Button _buttonDisconnectClient;
        [SerializeField] private Button _buttonSendMessage;
        [SerializeField] private TMP_Text _dataClientText;
        [SerializeField] private TMP_Text _consoleClientText;

        [Space]
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _textField;

        private void Start()
        {
            _textField.text = "";
            _dataClientText.text = "";
            _consoleClientText.text = "";

            _dataServerText.text = "";
            _consoleServerText.text = "";

            _buttonStartServer.onClick.AddListener(ServerStartHandler);
            _buttonStopServer.onClick.AddListener(ServerStopHandler);
            _buttonConnectClient.onClick.AddListener(ClientConnectHandler);
            _buttonDisconnectClient.onClick.AddListener(ClientDisconnectHandler);
            _buttonSendMessage.onClick.AddListener(ClientSendMessageHandler);

            _client.OnMessageReceive += ClientReceiveMessageHandler;
            _client.OnClientChangeState += OnClientChangeStateHandler;
            _client.OnClientData += OnClientDataHandler;
            _client.OnClientConsoleNewData += OnClientConsoleNewDataHandler;

            _server.OnServerChangeState += OnServerChangeStateHandler;
            _server.OnServerData += OnServerDataHandler;
            _server.OnServerConsoleNewData += OnServerConsoleNewDataHandler;
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
            _client.OnClientData -= OnClientDataHandler;
            _client.OnClientConsoleNewData -= OnClientConsoleNewDataHandler;

            _server.OnServerChangeState -= OnServerChangeStateHandler;
            _server.OnServerData -= OnServerDataHandler;
            _server.OnServerConsoleNewData -= OnServerConsoleNewDataHandler;
        }

        private void OnClientDataHandler(string data)
        {
            _dataClientText.text = data.Length == 0 ? "" : $"{data}\n{_dataClientText.text}";
        }

        private void OnClientConsoleNewDataHandler(string data)
        {
            _consoleClientText.text = $"{data}\n{_consoleClientText.text}";
        }

        private void OnServerDataHandler(string data)
        {
            _dataServerText.text = data.Length == 0 ? "" : $"{data}\n{_dataServerText.text}";
        }

        private void OnServerConsoleNewDataHandler(string data)
        {
            _consoleServerText.text = $"{data}\n{_consoleServerText.text}";
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
            _titleClient.color = state ? _activeColor : _inactiveColor;
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