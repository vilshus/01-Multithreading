using System;

using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;
using ChatLib;

namespace ChatServer
{
    public class ChatClientManager
    {
        public void StartServer()
        {
            while (true)
            {
                ReceiveConnection();
            }
        }

        private void ReceiveConnection()
        {
            var namedPipeServerStream = new NamedPipeServerStream("ChatPipe", PipeDirection.InOut);
            namedPipeServerStream.WaitForConnection();

            ConsoleMessageHelper.WriteSystemMessage("Connection received!");

            var clientUser = GetClientUser(namedPipeServerStream);
            namedPipeServerStream.Close();

            Task.Run(() =>
            {
                var chatClientConnection = new ChatClientConnection();
                if (chatClientConnection.EstablishConnection(clientUser))
                {
                    chatClientConnection.StartChatClientProcess();
                }
            });
        }

        private User GetClientUser(NamedPipeServerStream pipeServerStream)
        {
            try
            {
                StreamString pipeStringStream = new StreamString(pipeServerStream);
                string message = pipeStringStream.ReadString();

                if (message.Equals("Connection closed"))
                {
                    ConsoleMessageHelper.WriteSystemMessage("Connection terminated by client");
                    return null;
                }

                var clientUser = JsonSerializer.Deserialize<User>(message);

                ConsoleMessageHelper.WriteInfoMessage($"Client {clientUser.Name} wants to connect");
                pipeStringStream.WriteString("Client user received");

                return clientUser;
            }
            catch (Exception e)
            {
                ConsoleMessageHelper.WriteErrorMessage(e.Message);
                return null;
            }
        }
    }
}