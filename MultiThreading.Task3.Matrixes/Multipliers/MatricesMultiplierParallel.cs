using System.Threading;
using System.Threading.Tasks;
using MultiThreading.Task3.MatrixMultiplier.Matrices;

namespace MultiThreading.Task3.MatrixMultiplier.Multipliers
{
    public class MatricesMultiplierParallel : IMatricesMultiplier
    {
        public IMatrix Multiply(IMatrix m1, IMatrix m2)
        {
            var resultMatrix = new Matrix(m1.RowCount, m2.ColCount);

            Parallel.For(0, m1.RowCount,
                (i) => Parallel.For(0, m2.ColCount, (j) =>
                {
                    resultMatrix.SetElement(i, j, GetMultiplicationMatrixSingleValueNormal(m1, m2, i, j));
                }));

            return resultMatrix;
        }

        //Remark: Parallel calculation of one result matrix value is not efficient as it increases the execution time drastically.
        private long GetMultiplicationMatrixSingleValueParallel(IMatrix m1, IMatrix m2, long row, long column)
        {
            long sum = 0;
            Parallel.For<long>(0, m1.ColCount, () => 0, (i, loop, subtotal) =>
            {
                subtotal += m1.GetElement(row, i) * m2.GetElement(i, column);
                return subtotal;
            },
                (x) => Interlocked.Add(ref sum, x)
            );

            return sum;
        }

        private long GetMultiplicationMatrixSingleValueNormal(IMatrix m1, IMatrix m2, long row, long column)
        {
            long sum = 0;
            for (long k = 0; k < m1.ColCount; k++)
            {
                sum += m1.GetElement(row, k) * m2.GetElement(k, column);
            }

            return sum;
        }
    }
}
