/*
 * 5. Write a program which creates two threads and a shared collection:
 * the first one should add 10 elements into the collection and the second should print all elements
 * in the collection after each adding.
 * Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading.Task5.Threads.SharedCollection
{
    class Program
    {
        private static AutoResetEvent _readLock = new AutoResetEvent(false);
        private static AutoResetEvent _writeLock = new AutoResetEvent(true);
        
        private static List<int> _sharedCollection = new List<int>();
        private static int _maxCollectionSize = 10;

        static void Main(string[] args)
        {
            Console.WriteLine("5. Write a program which creates two threads and a shared collection:");
            Console.WriteLine("the first one should add 10 elements into the collection and the second should print all elements in the collection after each adding.");
            Console.WriteLine("Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.");
            Console.WriteLine();

            // feel free to add your code
            new TaskFactory().StartNew(() =>
            {
                for (int i = 1; i <= _maxCollectionSize; i++)
                {
                    _writeLock.WaitOne();
                    _sharedCollection.Add(i);
                    _readLock.Set();
                }
            });

            new TaskFactory().StartNew(() =>
            {
                for (int i = 1; i <= _maxCollectionSize; i++)
                {
                    _readLock.WaitOne();
                    Console.WriteLine(string.Join(", ", _sharedCollection));
                    _writeLock.Set();
                }
            });

            Console.ReadLine();
        }
    }
}
