using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LearningConcurrency.ch03
{
    public static class ParallelRecipes
    {
        /// <summary>
        /// Invoke a CPU intensive operation on each element of a collection spreading the work across
        /// available CPUs.
        /// </summary>
        /// <param name="matrices">A collection of matrices.</param>
        /// <param name="degress">The degree to rotate.</param>
        public static void RotateMatrices(IEnumerable<Matrix> matrices, int degress)
        {
            Parallel.ForEach(matrices, matrix => matrix.Rotate(degress));
        }

        /// <summary>
        /// Example of how to cancel a parallel execution. We can get a reference to the parallel loop state
        /// and call stop on it.
        /// </summary>
        /// <param name="matrices">The collection of matrices.</param>
        public static void InvertMatrices(IEnumerable<Matrix> matrices)
        {
            Parallel.ForEach(matrices, (matrix, state) =>
            {
                if (!matrix.IsInvertible)
                {
                    state.Stop();
                }
                else
                {
                    matrix.Invert();
                }
            });
        }

        /// <summary>
        /// An example of how to cancel a parallel loop execution from outside of the loop.
        /// This is done by passing a cancellation token to the loop.
        /// </summary>
        /// <param name="matrices"></param>
        /// <param name="degrees"></param>
        /// <param name="token"></param>
        public static void RotateMatrices(IEnumerable<Matrix> matrices, int degrees, CancellationToken token)
        {
            Parallel.ForEach(matrices, new ParallelOptions {CancellationToken = token}, matrix => matrix.Rotate(degrees));
        }

        public static int CountInvertMatrices(IEnumerable<Matrix> matrices)
        {
            object mutex = new object();
            int nonInvertibleCount = 0;
            Parallel.ForEach(matrices, matrix =>
            {
                if (matrix.IsInvertible)
                {
                    matrix.Invert();
                }
                else
                {
                    lock (mutex)
                    {
                        ++nonInvertibleCount;
                    }
                }
            });
            return nonInvertibleCount;
        }

        /// <summary>
        /// Example of how to create a parallel sum.
        /// </summary>
        /// <param name="values">Values to sum.</param>
        /// <returns></returns>
        public static int ParallelSum(IEnumerable<int> values)
        {
            object mutex = new object();
            int result = 0;
            Parallel.ForEach(
                source: values,
                localInit: () => 0,
                body: (item, state, localValue) => localValue + item,
                localFinally: localValue =>
                {
                    lock (mutex)
                    {
                        result += localValue;
                    }
                });
            return result;
        }

        /// <summary>
        /// Example of how to create a parallel sum with PLinq.
        /// </summary>
        /// <param name="values">The values to sum.</param>
        /// <returns></returns>
        public static int ParallelSumPLinq(IEnumerable<int> values)
        {
            return values.AsParallel().Sum();
        }

        /// <summary>
        /// Example of how to create a parallel sum with Plinq using the aggregate operation.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int ParallelSumPLinq2(IEnumerable<int> values)
        {
            return values.AsParallel().Aggregate(seed: 0, func: (sum, item) => sum + item);
        }

        /// <summary>
        /// Fake class to provide an example of CPU intensive operations.
        /// </summary>
        public class Matrix
        {
            public bool IsInvertible => true;

            public void Rotate(int degress)
            {
                Thread.Sleep(500);
            }

            public void Invert()
            {
                Thread.Sleep(500);
            }
        }
    }
}