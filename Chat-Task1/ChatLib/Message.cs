using System;

namespace ChatLib
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set;  }
        public User User { get; set; }
        public DateTime DateSent { get; set; }

        public Message()
        {
            Id = Guid.NewGuid();
        }

        public Message(User user, string content) : this()
        {
            Content = content;
            User = user;
        }
    }
}
