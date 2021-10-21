using System;

namespace ChatLib
{
    public static class ConsoleMessageHelper
    {
        public static void WriteInfoMessage(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static void WriteSystemMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        public static void WriteErrorMessage(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public static void WriteTitleMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message.ToUpperInvariant());
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
    }
}
