using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningConcurrency.ch03
{
    public static class QuickSort
    {
        public static void Sort(List<int> values)
        {
            InternalSort(values, 0, values.Count - 1);
        }

        public static void ParallelSort(List<int> values)
        {
            InternaParallellSort(values, 0, values.Count -1);
        }
        
        public static void InternaParallellSort(List<int> values, int begin, int end)
        {
            if (begin >= end) 
                return;
            // choose a pivot value
            int pivotIndex = begin;
            int pivotValue = values[pivotIndex];

            for (int index = begin+1; index <= end; index++)
            {
                if (values[index] < pivotValue)
                {
                    values[pivotIndex] = values[index];
                    pivotIndex += 1;
                    values[index] = values[pivotIndex];
                }
            }

            values[pivotIndex] = pivotValue;
            Parallel.Invoke(
                () => InternalSort(values, begin, pivotIndex - 1),
                () => InternalSort(values, pivotIndex + 1, end)
            );
        }

        public static void InternalSort(List<int> values, int begin, int end)
        {
            if (begin >= end) 
                return;
            // choose a pivot value
            int pivotIndex = begin;
            int pivotValue = values[pivotIndex];

            for (int index = begin+1; index <= end; index++)
            {
                if (values[index] < pivotValue)
                {
                    values[pivotIndex] = values[index];
                    pivotIndex += 1;
                    values[index] = values[pivotIndex];
                }
            }

            values[pivotIndex] = pivotValue;
            InternalSort(values, begin, pivotIndex - 1);
            InternalSort(values, pivotIndex + 1, end);
        }
    }
}