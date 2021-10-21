using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text.Json;
using ChatLib;

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

        public void SendMessage(Message message)
        {
            message.DateSent = DateTime.Now;
            var pipeStringStream = new StreamString(_pipeClientSend);
            pipeStringStream.WriteString(JsonSerializer.Serialize(message));
        }

        public string ReceiveMessages()
        {
            var pipeStringStream = new StreamString(_pipeClientReceive);
            var messagesString = pipeStringStream.ReadString();

            if (messagesString.Equals("Server disconnecting"))
            {
                return messagesString;
            }

            var messages = JsonSerializer.Deserialize<List<Message>>(messagesString);

            lock (_consoleLock)
            {
                foreach (var message in messages)
                {
                    ConsoleMessageHelper.WriteSystemMessage($"Client: {User.Name} received message");
                    DisplayMessage(message);
                }
            }

            return messagesString;
        }

        public void Disconnect()
        {
            var pipeStringStream = new StreamString(_pipeClientSend);
            pipeStringStream.WriteString("Connection closed");
            _pipeClientSend.Close();
            _pipeClientSend.Dispose();

            _pipeClientReceive.Close();
            _pipeClientReceive.Dispose();

            ConsoleMessageHelper.WriteSystemMessage($"Client: {User.Name} disconnected");
        }

        public void ServerDisconnected()
        {
            _pipeClientSend.Close();
            _pipeClientSend.Dispose();

            _pipeClientReceive.Close();
            _pipeClientReceive.Dispose();

            ConsoleMessageHelper.WriteSystemMessage($"Client: {User.Name} disconnected");
        }

        private void DisplayMessage(Message message)
        {
            lock (_consoleLock)
            {
                ConsoleMessageHelper.WriteInfoMessage($"{message.User.Name}, Date sent: {message.DateSent}");
                Console.WriteLine($"{message.Content}");
                Console.WriteLine();
            }
        }
    }
}
