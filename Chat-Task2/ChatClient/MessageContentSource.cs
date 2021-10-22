using System;
using System.Collections.Generic;

namespace ChatClient
{
    public class MessageContentSource
    {
        private const int NumberOfInitialMessages = 500;

        private readonly Random _random = new Random(1);
        private readonly List<string> _messages = new List<string>();

        public MessageContentSource()
        {
            for (int i = 0; i < NumberOfInitialMessages; i++)
            {
                _messages.Add($"Dummy message {_random.Next()}");
            }
        }

        public string GetRandomMessage()
        {
            return _messages[_random.Next(NumberOfInitialMessages)];
        }
    }
}
