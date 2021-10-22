using System;

namespace ChatLib
{
    public static class ConsoleMessageHelper
    {
        private static object _consoleLock = new object();

        public static void WriteInfoMessage(string message)
        {
            lock (_consoleLock)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(message);
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        public static void WriteSystemMessage(string message)
        {
            lock (_consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
        }

        public static void WriteErrorMessage(string message)
        {
            lock (_consoleLock)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        public static void WriteTitleMessage(string message)
        {
            lock (_consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message.ToUpperInvariant());
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
        }
    }
}
