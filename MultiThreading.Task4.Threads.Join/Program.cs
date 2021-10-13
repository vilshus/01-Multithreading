/*
 * 4.	Write a program which recursively creates 10 threads.
 * Each thread should be with the same body and receive a state with integer number, decrement it,
 * print and pass as a state into the newly created thread.
 * Use Thread class for this task and Join for waiting threads.
 * 
 * Implement all of the following options:
 * - a) Use Thread class for this task and Join for waiting threads.
 * - b) ThreadPool class for this task and Semaphore for waiting threads.
 */

using System;
using System.Threading;

namespace MultiThreading.Task4.Threads.Join
{
    class Program
    {
        private static readonly Semaphore DecrementIntegerAccessControl = new Semaphore(0, 1);

        static void Main(string[] args)
        {
            Console.WriteLine("4.	Write a program which recursively creates 10 threads.");
            Console.WriteLine("Each thread should be with the same body and receive a state with integer number, decrement it, print and pass as a state into the newly created thread.");
            Console.WriteLine("Implement all of the following options:");
            Console.WriteLine();
            Console.WriteLine("- a) Use Thread class for this task and Join for waiting threads.");
            Console.WriteLine("- b) ThreadPool class for this task and Semaphore for waiting threads.");

            Console.WriteLine();
            Console.WriteLine("a) Implementing with Join");
            DecrementIntegerWorkTaskWithJoin(10);

            Console.WriteLine();
            Console.WriteLine("b) Implementing with ThreadPool and Semaphore");
            DecrementIntegerWorkTaskWithSemaphore(10);

            Console.ReadKey();
        }

        private static void DecrementIntegerWorkTaskWithJoin(int counter)
        {
            if (counter <= 0)
            {
                return;
            }

            var thread = new Thread(() => {
                counter--;
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} decremented the number to {counter}");
            });
            thread.Start();
            thread.Join();
            DecrementIntegerWorkTaskWithJoin(counter);
        }

        private static void DecrementIntegerWorkTaskWithSemaphore(int counter)
        {
            if (counter <= 0)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(x => {
                counter--;
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} decremented the number to {counter}");
                DecrementIntegerAccessControl.Release(1);
            });

            DecrementIntegerAccessControl.WaitOne();
            DecrementIntegerWorkTaskWithSemaphore(counter);
        }
    }
}
