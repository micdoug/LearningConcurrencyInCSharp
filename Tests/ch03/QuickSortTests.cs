using System.Collections.Generic;
using Xunit;

namespace LearningConcurrency.ch03
{
    public class QuickSortTests
    {
        [Fact]
        public void TestPivot()
        {
            List<int> values = new List<int> {2, 1, 1, 1, 3, 3, 3};
            QuickSort.InternalSort(values, 0, values.Count -1);
            int index = values.FindIndex(value => value == 2);
            Assert.Equal(3, index);
        }

        [Fact]
        public void TestSorting()
        {
            List<int> values = new List<int> { 5, 7, 3, 6, 10, -2, 0, 20, 4, 0, 1, 32 };
            QuickSort.Sort(values);
            for (int i = 0; i < values.Count - 1; i++)
            {
                Assert.True(values[i] <= values[i+1]);
            }
        }
        
        [Fact]
        public void TestParallelSorting()
        {
            List<int> values = new List<int> { 5, 7, 3, 6, 10, -2, 0, 20, 4, 0, 1, 32 };
            QuickSort.ParallelSort(values);
            for (int i = 0; i < values.Count - 1; i++)
            {
                Assert.True(values[i] <= values[i+1]);
            }
        }
    }
}