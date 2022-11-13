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
        [SerializeField] private Transform _chatMenu;
        [SerializeField] private Transform _loginMenu;

        [Space]
        [SerializeField] private TMP_InputField _loginInputField;
        [SerializeField] private Button _loginButton;

        [Space]
        [Space]
        [SerializeField] private Server _server;
        [SerializeField] private TMP_Text _titleServer;
        [SerializeField] private Button _buttonSetServerData;
        [SerializeField] private TMP_InputField _serverIP;
        [SerializeField] private TMP_InputField _serverPort;
        [SerializeField] private Button _buttonStartServer;
        [SerializeField] private Button _buttonStopServer;
        [SerializeField] private TMP_Text _dataServerText;
        [SerializeField] private TMP_Text _consoleServerText;

        [Space]
        [Space]
        [SerializeField] private Client _client;
        [SerializeField] private TMP_Text _titleClient;
        [SerializeField] private Button _buttonSetClientData;
        [SerializeField] private TMP_InputField _clientIP;
        [SerializeField] private TMP_InputField _clientPort;
        [SerializeField] private Button _buttonConnectClient;
        [SerializeField] private Button _buttonDisconnectClient;
        [SerializeField] private TMP_Text _dataClientText;
        [SerializeField] private TMP_Text _consoleClientText;

        [Space]
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private TMP_Text _clientLogin;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _buttonSendMessage;

        private void Start()
        {
            _textField.text = "";
            _dataClientText.text = "";
            _consoleClientText.text = "";

            _dataServerText.text = "";
            _consoleServerText.text = "";

            _serverIP.text = _server.ServerIP;
            _serverPort.text = _server.ServerPort.ToString();

            _clientIP.text = _client.ClientIP;
            _clientPort.text = _client.ClientPort.ToString();

            _client.ServerIP = _server.ServerIP;
            _client.ServerPort = _server.ServerPort;

            _buttonSetServerData.onClick.AddListener(SetServerDataHandler);
            _buttonStartServer.onClick.AddListener(ServerStartHandler);
            _buttonStopServer.onClick.AddListener(ServerStopHandler);

            _buttonSetClientData.onClick.AddListener(SetClientDataHandler);
            _buttonConnectClient.onClick.AddListener(ClientConnectHandler);
            _buttonDisconnectClient.onClick.AddListener(ClientDisconnectHandler);

            _loginInputField.onEndEdit.AddListener(LoginInputHandler);
            _loginButton.onClick.AddListener(() => { LoginInputHandler(_loginInputField.text); });

            _inputField.onEndEdit.AddListener(ClientSendMessageHandler);
            //_buttonSendMessage.onClick.AddListener(delegate { ClientSendMessageHandler(_inputField.text); });
            _buttonSendMessage.onClick.AddListener(() => { ClientSendMessageHandler(_inputField.text); });

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
            _buttonSetServerData.onClick.RemoveAllListeners();
            _buttonStartServer.onClick.RemoveAllListeners();
            _buttonStopServer.onClick.RemoveAllListeners();
            
            _buttonSetClientData.onClick.RemoveAllListeners();
            _buttonConnectClient.onClick.RemoveAllListeners();
            _buttonDisconnectClient.onClick.RemoveAllListeners();

            _loginInputField.onEndEdit.RemoveAllListeners();
            _loginButton.onClick.RemoveAllListeners();

            _inputField.onEndEdit.RemoveAllListeners();
            _buttonSendMessage.onClick.RemoveAllListeners();

            _client.OnMessageReceive -= ClientReceiveMessageHandler;
            _client.OnClientChangeState -= OnClientChangeStateHandler;
            _client.OnClientData -= OnClientDataHandler;
            _client.OnClientConsoleNewData -= OnClientConsoleNewDataHandler;

            _server.OnServerChangeState -= OnServerChangeStateHandler;
            _server.OnServerData -= OnServerDataHandler;
            _server.OnServerConsoleNewData -= OnServerConsoleNewDataHandler;
        }

        private void SetServerDataHandler()
        {
            _server.ServerIP = _serverIP.text;
            _server.ServerPort = int.Parse(_serverPort.text);
            _client.ServerIP = _server.ServerIP;
            _client.ServerPort = _server.ServerPort;
        }

        private void SetClientDataHandler()
        {
            _client.ClientIP = _clientIP.text;
            _client.ClientPort = int.Parse(_clientPort.text);
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
            _chatMenu.gameObject.SetActive(state);
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
            _loginMenu.gameObject.SetActive(true);
        }

        private void LoginInputHandler(string login)
        {
            _loginMenu.gameObject.SetActive(false);
            _clientLogin.text = login;
            _client.ClientConnect(login);
        }

        private void ClientDisconnectHandler()
        {
            _client.ClientDisconnect();
        }

        private void ClientSendMessageHandler(string message)
        {
            _client.ClientSendMessage(message);
            _inputField.text = "";
        }

        public void ClientReceiveMessageHandler(string message)
        {
            _textField.text = $"{message}\n{_textField.text}";
        }
    }
}