using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading.Tasks.Dataflow;

namespace LearningConcurrency.ch01
{
    public static class Ch01Lessons
    {
        public static async Task ResumeContext()
        {
            Console.WriteLine($"It is running on thread {Thread.CurrentThread.ManagedThreadId}");
            int theAnswer = 42;
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(continueOnCapturedContext: true);
            Console.WriteLine($"Now it is running on thread {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"The real answer is {theAnswer}");
        }

        public static async Task DontResumeContext()
        {
            Console.WriteLine($"It is running on thread {Thread.CurrentThread.ManagedThreadId}");
            int theAnswer = 42;
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(continueOnCapturedContext: false);
            Console.WriteLine($"Now it is running on thread {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"The real answer is {theAnswer}");
        }

        public static void IsEvenParallel()
        {
            IEnumerable<int> numbers = Enumerable.Range(0, 100);
            Parallel.ForEach(numbers, x =>
            {
                if (x % 2 == 0)
                {
                    Console.WriteLine($"{x} is even");
                }
                else
                {
                    Console.WriteLine($"{x} is odd");
                }
            });
        }

        public static async Task CountEvenSeconds()
        {
            IObservable<DateTimeOffset> timestamps = Observable.Interval(TimeSpan.FromSeconds(1))
                .Timestamp()
                .Where(x => x.Value % 2 == 0)
                .Select(x => x.Timestamp);

            timestamps.Subscribe(x => Console.WriteLine(x));

            await Task.Delay(TimeSpan.FromSeconds(10));
                
        }

        public static async Task DataFlowIntro()
        {
            try
            {
                var multiplyBlock = new TransformBlock<int, int>(item =>
                {
                    if (item == 1)
                    {
                        throw new InvalidOperationException("Blech.");
                    }

                    return item * 2;
                });
                var subtractBlock = new TransformBlock<int, int>(item => item - 2);
                multiplyBlock.LinkTo(subtractBlock, new DataflowLinkOptions{PropagateCompletion = true});

                multiplyBlock.Post(10);
                await subtractBlock.Completion;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}