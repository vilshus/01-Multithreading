/*
 * 3. Write a program, which multiplies two matrices and uses class Parallel.
 * a. Implement logic of MatricesMultiplierParallel.cs
 *    Make sure that all the tests within MultiThreading.Task3.MatrixMultiplier.Tests.csproj run successfully.
 * b. Create a test inside MultiThreading.Task3.MatrixMultiplier.Tests.csproj to check which multiplier runs faster.
 *    Find out the size which makes parallel multiplication more effective than the regular one.
 */

using System;
using MultiThreading.Task3.MatrixMultiplier.Matrices;
using MultiThreading.Task3.MatrixMultiplier.Multipliers;

namespace MultiThreading.Task3.MatrixMultiplier
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("3.	Write a program, which multiplies two matrices and uses class Parallel. ");
            Console.WriteLine();

            long matrixSize;
            bool validInput;
            do
            {
                Console.Write("Size of the matrices (long > 0): ");
                var matrixSizeInput = Console.ReadLine();
                validInput = Int64.TryParse(matrixSizeInput, out matrixSize);
                validInput = validInput && matrixSize > 0;
            } while (!validInput);

            CreateAndProcessMatrices(matrixSize);
            Console.ReadLine();
        }

        private static void CreateAndProcessMatrices(long sizeOfMatrix)
        {
            Console.WriteLine("Multiplying...");
            var firstMatrix = new Matrix(sizeOfMatrix, sizeOfMatrix, true);
            var secondMatrix = new Matrix(sizeOfMatrix, sizeOfMatrix, true);

            //IMatrix resultMatrix = new MatricesMultiplier().Multiply(firstMatrix, secondMatrix);
            IMatrix resultMatrix = new MatricesMultiplierParallel().Multiply(firstMatrix, secondMatrix);

            Console.WriteLine("firstMatrix:");
            firstMatrix.Print();
            Console.WriteLine("secondMatrix:");
            secondMatrix.Print();
            Console.WriteLine("resultMatrix:");
            resultMatrix.Print();
        }
    }
}
