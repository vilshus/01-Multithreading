using ChatLib;

namespace ChatClient
{
    public interface IChatClient
    {
        User User { get; }

        Message CreateMessage(string content);

        void SendMessage(Message message);

        void ReceiveMessages();

        void Disconnect();

        bool IsConnected();
    }
}
