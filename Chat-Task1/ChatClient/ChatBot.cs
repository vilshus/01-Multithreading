using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatLib;

namespace ChatClient
{
    public class ChatBot
    {
        
        private const int MaxNumberOfMessagesPerClient = 5;
        private const int MaxMillisecondsBetweenMessages = 2000;

        private static object _usersLock = new object();

        private Random _random = new Random();
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private readonly MessageContentSource _messageContentSource;

        public ChatBot(MessageContentSource messageContentSource)
        {
            _messageContentSource = messageContentSource;
        }

        public void DoWork()
        {
            while (true)
            {
                var task = new Task(NewClient, TaskCreationOptions.LongRunning);
                task.Start();
                Thread.Sleep(1000); //launch new client connection every 1 sec
            }
        }

        private void NewClient()
        {
            User user;
            lock (_usersLock)
            {
                user = new User($"User {_users.Count + 1}");
                _users.Add(user.Name, user);
            }

            var client = new ChatClientConsole(user);

            var numberOfMessages = _random.Next(1, MaxNumberOfMessagesPerClient + 1);
            client.ReceiveMessages();

            for (int i = 0; i < numberOfMessages; i++)
            {
                var message = client.CreateMessage(_messageContentSource.GetRandomMessage());
                client.SendMessage(message);
                var serverMessages = client.ReceiveMessages();
                if (serverMessages.Equals("Server disconnecting"))
                {
                    ConsoleMessageHelper.WriteErrorMessage("Server disconnecting");
                    client.ServerDisconnected();
                    return;
                }
                Thread.Sleep(_random.Next(MaxMillisecondsBetweenMessages));
            }
            client.Disconnect();
        }
    }
}
