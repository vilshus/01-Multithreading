using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatLib;

namespace ChatServer
{
    public class ChatClientConnection
    {
        private const int ClientChatHistoryNumberOfMessages = 20;

        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static object _consoleLock = new object();
        private static object _newMessagesLock = new object();

        private static List<Message> _chatHistory = new List<Message>();
        private static Dictionary<Guid, List<Message>> _clientNewMessages = new Dictionary<Guid, List<Message>>();

        private static List<UserServerConnection> _activeUserConnections = new List<UserServerConnection>();
        private UserServerConnection _userConnection;
        //private NamedPipeServerStream _namedPipeServerStreamReceive;
        //private NamedPipeServerStream _namedPipeServerStreamSend;
        //private User _clientUser;

        public bool EstablishConnection(User clientUser)
        {
            try
            {
                _userConnection = new UserServerConnection();
                _userConnection.ReceivePipe = new NamedPipeServerStream($"ClientSend:{clientUser.Id}", PipeDirection.InOut);
                _userConnection.ReceivePipe.WaitForConnection();

                _userConnection.SendPipe = new NamedPipeServerStream($"ClientReceive:{clientUser.Id}", PipeDirection.InOut);
                _userConnection.SendPipe.WaitForConnection();
                _userConnection.User = clientUser;
                _activeUserConnections.Add(_userConnection);

                SetConsoleCtrlHandler(ConsoleEventCallback, true);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void StartChatClientProcess()
        {
            if (!(_userConnection.ReceivePipe is {IsConnected: true}) || !(_userConnection.SendPipe is { IsConnected: true }))
            {
                _activeUserConnections.Remove(_userConnection);
                throw new InvalidOperationException("Connection should be established before starting client processes.");
            }

            ConsoleMessageHelper.WriteSystemMessage($"Client {_userConnection.User.Name} connected");

            lock (_newMessagesLock)
            {
                _clientNewMessages.Add(_userConnection.User.Id, new List<Message>());
                SendMessages(_userConnection.SendPipe, _chatHistory.TakeLast(ClientChatHistoryNumberOfMessages).ToList());
            }

            while (_userConnection.ReceivePipe.IsConnected)
            {
                try
                {
                    var stringStreamRead = new StreamString(_userConnection.ReceivePipe);
                    var messageString = stringStreamRead.ReadString();

                    if (messageString.Equals("Connection closed"))
                    {
                        ConsoleMessageHelper.WriteSystemMessage($"Client {_userConnection.User.Name} disconnected");
                        break;
                    }

                    var message = JsonSerializer.Deserialize<Message>(messageString);
                    RegisterMessage(message);
                    SendNewMessages(_userConnection.SendPipe, _userConnection.User);

                    lock (_consoleLock)
                    {
                        ConsoleMessageHelper.WriteInfoMessage($"Message received from: {_userConnection.User.Name}");
                        Console.WriteLine(message.Content);
                    }
                }
                catch (Exception e)
                {
                    ConsoleMessageHelper.WriteErrorMessage(e.Message);
                }
            }

            _activeUserConnections.Remove(_userConnection);
            _userConnection.ReceivePipe.Close();
            _userConnection.ReceivePipe.Dispose();
            _userConnection.SendPipe.Close();
            _userConnection.SendPipe.Dispose();
        }

        private static void RegisterMessage(Message message)
        {
            lock (_newMessagesLock)
            {
                _chatHistory.Add(message);
                foreach (var newMessages in _clientNewMessages.Values)
                {
                    newMessages.Add(message);
                }
            }
        }

        private void SendNewMessages(NamedPipeServerStream pipeServerStream, User user)
        {
            lock (_newMessagesLock)
            {
                SendMessages(pipeServerStream, _clientNewMessages[user.Id]);
                _clientNewMessages[user.Id].Clear();
            }
        }

        private void SendMessages(NamedPipeServerStream pipeServerStream, List<Message> messages)
        {
            StreamString stringStreamWrite = new StreamString(pipeServerStream);
            stringStreamWrite.WriteString(JsonSerializer.Serialize(messages));
        }

        public static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                ConsoleMessageHelper.WriteErrorMessage("Server disconnecting");
                var tasks = new List<Task>();
                foreach (var userConnection in _activeUserConnections)
                {
                    tasks.Add(InformClientAndCloseConnection(userConnection));
                }

                Task.WaitAll(tasks.ToArray());
            }
            return false;
        }

        private static Task InformClientAndCloseConnection(UserServerConnection userConnection)
        {
            StreamString stringStreamWrite = new StreamString(userConnection.SendPipe);
            Task.Run(() => stringStreamWrite.WriteString("Server disconnecting"));

            StreamString stringStreamReader = new StreamString(userConnection.ReceivePipe);

            //Read and save last message
            Task.Run(() =>
            {
                var messageString = stringStreamReader.ReadString();
                Message message;
                try
                {
                    message = JsonSerializer.Deserialize<Message>(messageString);
                }
                catch (Exception)
                {
                    return;
                }

                RegisterMessage(message);
            });

            return Task.Run(() =>
            {
                //give 2 sec for the client to disconnect
                var timeCounter = 2000;
                while (userConnection.ReceivePipe.IsConnected && timeCounter > 0)
                {
                    timeCounter -= 100;
                    Thread.Sleep(100);
                }

                userConnection.ReceivePipe.Close();
                userConnection.ReceivePipe.Dispose();
                userConnection.SendPipe.Close();
                userConnection.SendPipe.Dispose();
            });
        }
    }
}
