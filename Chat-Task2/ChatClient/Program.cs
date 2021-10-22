using System;
using ChatLib;

namespace ChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleMessageHelper.WriteTitleMessage("CHAT CLIENT BOT");
            Console.WriteLine();
            var chatBot = new ChatBot(new MessageContentSource());
            chatBot.DoWork();

            Console.WriteLine();
            ConsoleMessageHelper.WriteTitleMessage("Work finished");
            Console.ReadLine();
        }
    }
}
