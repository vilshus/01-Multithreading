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

        private Random _random = new Random(1);
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
                Task.Run(NewClient);
                Thread.Sleep(1000); //launch new client connection every 1 sec
            }

            // for debugging
            /*var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(NewClient));
                //Thread.Sleep(1000); //launch new client connection every 1 sec
            }

            Task.WaitAll(tasks.ToArray());*/
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

            // get chat history first
            Task.Run(() => client.ReceiveMessages()).Wait();

            //start listening to server
            var listeningWorker = new Task(() => ListenToServer(client), TaskCreationOptions.LongRunning);
            listeningWorker.Start();

            for (int i = 0; i < numberOfMessages; i++)
            {
                var message = client.CreateMessage(_messageContentSource.GetRandomMessage());
                client.SendMessageAsync(message).Start();
                Thread.Sleep(_random.Next(MaxMillisecondsBetweenMessages));
            }

            client.Disconnect();
        }

        private void ListenToServer(ChatClientConsole client)
        {
            while (client.IsConnected())
            {
                client.ReceiveMessages();
            }
        }
    }
}
