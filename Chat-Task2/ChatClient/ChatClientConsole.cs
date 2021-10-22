using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatLib;
using Microsoft.VisualBasic;

namespace ChatClient
{
    public class ChatClientConsole : IChatClient
    {
        private static object _consoleLock = new object();
        private NamedPipeClientStream _pipeClientSend;
        private NamedPipeClientStream _pipeClientReceive;

        public User User { get; }

        public ChatClientConsole(User user)
        {
            User = user;
            _pipeClientSend =
                new NamedPipeClientStream(".", "ChatPipe",
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

            _pipeClientSend.Connect();
            var pipeStringStream = new StreamString(_pipeClientSend);
            pipeStringStream.WriteString(JsonSerializer.Serialize(User));
            var response = pipeStringStream.ReadString();
            if (response.Equals("Client user received"))
            {
                _pipeClientSend.Close();
                _pipeClientSend.Dispose();
            }

            _pipeClientSend =
                new NamedPipeClientStream(".", $"ClientSend:{user.Id}",
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

            _pipeClientSend.Connect();

            _pipeClientReceive =
                new NamedPipeClientStream(".", $"ClientReceive:{user.Id}",
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation);

            _pipeClientReceive.Connect();
            if (_pipeClientReceive.IsConnected)
            {
                ConsoleMessageHelper.WriteSystemMessage($"Client: {user.Name} connected");
            }
        }

        public Message CreateMessage(string content)
        {
            return new Message(User, content);
        }

        public Task SendMessageAsync(Message message)
        {
            return new Task(() => SendMessage(message));
        }

        public void SendMessage(Message message)
        {
            if (!IsConnected())
            {
                return;
            }

            message.DateSent = DateTime.Now;
            var pipeStringStream = new StreamString(_pipeClientSend);
            pipeStringStream.WriteString(JsonSerializer.Serialize(message));
        }

        public void ReceiveMessages()
        {
            if (!IsConnected())
            {
                return;
            }

            var pipeStringStream = new StreamString(_pipeClientReceive);
            var messagesString = pipeStringStream.ReadString();

            if (messagesString.Equals("Server disconnecting"))
            {
                ConsoleMessageHelper.WriteErrorMessage("Server disconnecting");
                ServerDisconnected();
                return;
            }

            List<Message> messages;
            try
            {
                messages = JsonSerializer.Deserialize<List<Message>>(messagesString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            lock (_consoleLock)
            {
                foreach (var message in messages)
                {
                    ConsoleMessageHelper.WriteSystemMessage($"Client: {User.Name} received message");
                    DisplayMessage(message);
                }
            }
        }

        public async void Disconnect()
        {
            if (_pipeClientSend.IsConnected)
            {
                var pipeStringStream = new StreamString(_pipeClientSend);
                pipeStringStream.WriteString("Connection closed");
                _pipeClientSend.Close();
                await _pipeClientSend.DisposeAsync();
            }

            if (_pipeClientReceive.IsConnected)
            {
                _pipeClientReceive.Close();
                await _pipeClientReceive.DisposeAsync();
            }

            ConsoleMessageHelper.WriteSystemMessage($"Client: {User.Name} disconnected");
        }

        public async void ServerDisconnected()
        {
            if (_pipeClientSend.IsConnected)
            {
                _pipeClientSend.Close();
                await _pipeClientSend.DisposeAsync();
            }

            if (_pipeClientReceive.IsConnected)
            {
                _pipeClientReceive.Close();
                await _pipeClientReceive.DisposeAsync();
            }

            ConsoleMessageHelper.WriteSystemMessage($"Client: {User.Name} disconnected");
        }

        public bool IsConnected()
        {
            return _pipeClientSend.IsConnected && _pipeClientReceive.IsConnected;
        }

        private void DisplayMessage(Message message)
        {
            if (User.Name != "User 5" && User.Name != "User 4")
            {
                return;
            }

            lock (_consoleLock)
            {
                ConsoleMessageHelper.WriteInfoMessage($"{message.User.Name}, Date sent: {message.DateSent}");
                Console.WriteLine($"{message.Content}");
                Console.WriteLine();
            }
        }
    }
}
