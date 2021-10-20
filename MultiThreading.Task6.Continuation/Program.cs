/*
*  Create a Task and attach continuations to it according to the following criteria:
   a.    Continuation task should be executed regardless of the result of the parent task.
   b.    Continuation task should be executed when the parent task finished without success.
   c.    Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation
   d.    Continuation task should be executed outside of the thread pool when the parent task would be cancelled
   Demonstrate the work of the each case with console utility.
*/
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading.Task6.Continuation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Create a Task and attach continuations to it according to the following criteria:");
            Console.WriteLine("a.    Continuation task should be executed regardless of the result of the parent task.");
            Console.WriteLine("b.    Continuation task should be executed when the parent task finished without success.");
            Console.WriteLine("c.    Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation.");
            Console.WriteLine("d.    Continuation task should be executed outside of the thread pool when the parent task would be cancelled.");
            Console.WriteLine("Demonstrate the work of the each case with console utility.");
            Console.WriteLine();

            // feel free to add your code
            Console.WriteLine();
            ExampleA();
            Console.WriteLine();
            ExampleB();
            Console.WriteLine();
            ExampleC();
            Console.WriteLine();
            ExampleD();

            Console.ReadLine();
        }

        private static void ExampleA()
        {
            PrintSubtaskHeader("a) Continuation task should be executed regardless of the result of the parent task.");

            var parentTask = new Task(() =>
            {
                Console.WriteLine("Parent task: Exception is thrown to demonstrate that continuation task is executed anyway");
                throw new Exception();
            });
            var continuationTask = parentTask.ContinueWith(previousTask => Console.WriteLine("Continuation task: Continuation task is executed."));

            parentTask.Start();
            continuationTask.Wait();
        }

        private static void ExampleB()
        {
            PrintSubtaskHeader("b) Continuation task should be executed when the parent task finished without success.");

            // Continuation task is not executed
            var parentTask = new Task(() => Console.WriteLine("Parent task: Parent task executed successfully. Continuation will not be executed."));
            var continuationTask = parentTask.ContinueWith(previousTask => Console.WriteLine("Continuation task: Executed."), TaskContinuationOptions.OnlyOnFaulted);
            parentTask.Start();
            try
            {
                Task.WaitAll(parentTask, continuationTask);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine();

            // Continuation task is executed
            parentTask = new Task(() =>
            {
                Console.WriteLine("Parent task: Parent task throws an exception. Continuation will be executed.");
                throw new Exception();
            });
            continuationTask = parentTask.ContinueWith(previousTask => Console.WriteLine("Continuation task: Executed."), TaskContinuationOptions.OnlyOnFaulted);
            parentTask.Start();
            continuationTask.Wait();
        }

        private static void ExampleC()
        {
            PrintSubtaskHeader("c) Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation.");

            var parentTask = new Task(() =>
            {
                Console.WriteLine($"Parent task: Task runs on thread {Thread.CurrentThread.ManagedThreadId} throws an exception. Continuation will be executed.");
                throw new Exception();
            });
            var continuationTask = parentTask.ContinueWith(previousTask =>
            {
                Console.WriteLine($"Continuation task: Continuation is executed on thread {Thread.CurrentThread.ManagedThreadId}");
            }, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);

            parentTask.Start();
            continuationTask.Wait();
        }

        private static void ExampleD()
        {
            PrintSubtaskHeader("d) Continuation task should be executed outside of the thread pool when the parent task would be cancelled.");

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'x')
                    {
                        cancellationTokenSource.Cancel();
                        return;
                    }
                }
            }, cancellationToken);

            var parentTask = new Task(() =>
            {
                Console.WriteLine($"Parent task: Task runs on thread IsThreadPool={Thread.CurrentThread.IsThreadPoolThread} until it is cancelled. Continuation will be executed.");
                Console.WriteLine("Press 'x' key to cancel the execution of the parent task.");
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);

            var continuationTask =
                parentTask.ContinueWith(previousTask => Console.WriteLine($"Continuation task: Continuation is executed on thread IsThreadPool={Thread.CurrentThread.IsThreadPoolThread}"),
                    TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.LongRunning);

            parentTask.Start();
            continuationTask.Wait();
        }


        private static void PrintSubtaskHeader(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
