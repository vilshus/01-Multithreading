/*
 * 2.	Write a program, which creates a chain of four Tasks.
 * First Task – creates an array of 10 random integer.
 * Second Task – multiplies this array with another random integer.
 * Third Task – sorts this array by ascending.
 * Fourth Task – calculates the average value. All this tasks should print the values to console.
 */
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiThreading.Task2.Chaining
{
    class Program
    {
        private const int ArraySize = 10;

        private static readonly Random Random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine(".Net Mentoring Program. MultiThreading V1 ");
            Console.WriteLine("2.	Write a program, which creates a chain of four Tasks.");
            Console.WriteLine("First Task – creates an array of 10 random integer.");
            Console.WriteLine("Second Task – multiplies this array with another random integer.");
            Console.WriteLine("Third Task – sorts this array by ascending.");
            Console.WriteLine("Fourth Task – calculates the average value. All this tasks should print the values to console");
            Console.WriteLine();

            var parentTask = new Task<int[]>(CreateRandomArray);
            var finalTask = parentTask.ContinueWith(previousTask => MultiplyByRandomNumber(previousTask.Result))
                .ContinueWith(previousTask => OrderArrayAscending(previousTask.Result))
                .ContinueWith(previousTask => CalculateAverage(previousTask.Result));

            parentTask.Start();
            finalTask.Wait();

            Console.ReadLine();
        }

        private static int[] CreateRandomArray()
        {
            Console.WriteLine("Task 1");
            var integerNumbers = new int[ArraySize];
            for (int i = 0; i < integerNumbers.Length; i++)
            {
                integerNumbers[i] = Random.Next(1000);
                Console.WriteLine(integerNumbers[i]);
            }
            Console.WriteLine();
            return integerNumbers;
        }

        private static int[] MultiplyByRandomNumber(int[] integerNumbers)
        {
            Console.WriteLine("Task 2");
            var randomNumber = Random.Next(1000);
            for (int i = 0; i < integerNumbers.Length; i++)
            {
                integerNumbers[i] = integerNumbers[i] * randomNumber;
                Console.WriteLine(integerNumbers[i]);
            }
            Console.WriteLine();
            return integerNumbers;
        }

        private static int[] OrderArrayAscending(int[] integerNumbers)
        {
            Console.WriteLine("Task 3");
            integerNumbers = integerNumbers.OrderBy(x => x).ToArray();
            foreach (var number in integerNumbers)
            {
                Console.WriteLine(number);
            }
            Console.WriteLine();
            return integerNumbers;
        }

        private static double CalculateAverage(int[] integerNumbers)
        {
            Console.WriteLine("Task 4");
            var average = integerNumbers.Average();
            Console.WriteLine(average);
            Console.WriteLine();
            return average;
        }
    }
}
