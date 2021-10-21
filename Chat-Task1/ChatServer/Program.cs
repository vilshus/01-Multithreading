using System;
using ChatLib;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleMessageHelper.WriteTitleMessage("CHAT SERVER");
            Console.WriteLine();
            var chatServer = new ChatClientManager();
            chatServer.StartServer();
            Console.ReadLine();
        }
    }
}
