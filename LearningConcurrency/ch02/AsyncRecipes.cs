using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace LearningConcurrency.ch02
{
    public static class AsyncRecipes
    {
        
        /// <summary>
        /// Example of how to simulate a pausing in an asynchronous method.
        /// </summary>
        public static async Task PausingForAPeriod()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Uses exponential back-off to download a string content from the provided uri.
        /// </summary>
        /// <param name="uri">The uri to load the content from.</param>
        public static async Task<string> DownloadStringWithRetries(string uri)
        {
            using(HttpClient client = new HttpClient())
            {
                // uses exponential back-off
                TimeSpan nextDelay = TimeSpan.FromSeconds(1);
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        return await client.GetStringAsync(uri);
                    }
                    catch
                    {
                        await Task.Delay(nextDelay);
                        nextDelay *= 2;
                    }
                }
                return await client.GetStringAsync(uri);
            }
        }

        /// <summary>
        /// Tries to download a string from a provided link. If a result is not provided
        /// during the provided timeout, a null result is returned.
        /// </summary>
        /// <param name="uri">The download uri.</param>
        /// <param name="timeout">The timeout value to use.</param>
        /// <returns>The downloaded string.</returns>
        public static async Task<string> DownloadStringWithTimeout(string uri, TimeSpan timeout)
        {
            using (HttpClient client = new HttpClient())
            {
                Task<string> downloadTask = client.GetStringAsync(uri);
                Task timeoutTask = Task.Delay(timeout);

                Task completedTask = await Task.WhenAny(downloadTask, timeoutTask);

                return completedTask == downloadTask ? downloadTask.Result : null;
            }
        }


        #region Here is an example of how to implement an asynchronous interface with synchronous code.
        interface MyAsyncInterface
        {
            Task<int> GetValueAsync();
        }

        class MySynchronousImplementation : MyAsyncInterface
        {
            public Task<int> GetValueAsync()
            {
                return Task.FromResult(42);
            }
        }
        #endregion

        /// <summary>
        /// How to implement an asynchronous method returning an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static Task<T> NotImplementedAsync<T>()
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            tcs.SetException(new NotImplementedException());
            return tcs.Task;
        }

        #region Reporting Progress
        
        /// <summary>
        /// Simulates a method with a intensive CPU calculation that reports its progress
        /// to the caller.
        /// </summary>
        /// <param name="progress">The progress instance that will receive percentage report.</param>
        static async Task ReportingProgress(IProgress<double> progress = null)
        {
            const int totalSeconds = 5;
            TimeSpan totalTime = TimeSpan.FromSeconds(totalSeconds);
            TimeSpan timeToComplete = TimeSpan.FromSeconds(totalSeconds);
            Random random = new Random();
            while (timeToComplete > TimeSpan.Zero)
            {
                TimeSpan randomTime = TimeSpan.FromMilliseconds(random.Next(1000));
                randomTime = randomTime < timeToComplete ? randomTime : timeToComplete;
                Console.WriteLine($"Waiting for {randomTime}.");
                await Task.Delay(randomTime);
                timeToComplete -= randomTime;
                Console.WriteLine($"Time to complete {timeToComplete}.");
                progress?.Report(100 * (totalTime-timeToComplete)/totalTime);
            }
            
        }

        /// <summary>
        /// Show how to capture the progress from a method that supports it.
        /// </summary>
        public static async Task GettingProgress()
        {
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, completed) => Console.WriteLine($"Progress {completed}%");
            await ReportingProgress(progress);
        }
        
        #endregion

        #region Waiting multiple tasks
        
        /// <summary>
        /// Show how to wait for a collection of tasks to complete.
        /// </summary>
        /// <returns></returns>
        public static async Task WaitingSeriaOfTasksCompletion()
        {
            Task task1 = Task.Delay(1);
            Task task2 = Task.Delay(2);
            Task task3 = Task.Delay(3);

            await Task.WhenAll(task1, task2, task3);
        }

        /// <summary>
        /// Downloads the html content of multiple pages at the same time.
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        static async Task<string> DownloadAllAsync(IEnumerable<string> urls)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // Create a download task for each url
                var downloads = urls.Select(url => httpClient.GetStringAsync(url));
                // at this point no tasks have actually started
                Task<string>[] downloadTasks = downloads.ToArray();
                string[] htmlPages = await Task.WhenAll(downloadTasks);
                return String.Concat(htmlPages);
            }
        }

        static Task ThrowNotImplementedException()
        {
            throw new NotImplementedException();
        }

        static Task ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// When awaiting multiple tasks only the first thrown exception is caught.
        /// </summary>
        /// <returns></returns>
        public static async Task HandleMultipleExceptions()
        {
            try
            {
                Task task1 = ThrowNotImplementedException();
                Task task2 = ThrowInvalidOperationException();

                await Task.WhenAll(task1, task2);
            }
            catch (Exception exception)
            {
                // "exception" is either NotImplementedException or InvalidOperationException
                Console.WriteLine(exception);
            }
        }

        public static async Task CapturingMultipleExceptions()
        {
            Task task1 = ThrowNotImplementedException();
            Task task2 = ThrowInvalidOperationException();
            Task allTasks = Task.WhenAll(task1, task2);

            try
            {
                await allTasks;
            }
            catch (Exception)
            {
                AggregateException aggregateException = allTasks.Exception;
                Console.WriteLine(aggregateException);
            }
        }
        
        #endregion
        
        #region Waiting any task to complete

        /// <summary>
        /// Try to get the content from the two provided urls and return the result from the first one
        /// that we got a response for.
        /// </summary>
        /// <param name="urlA">The first url.</param>
        /// <param name="urlB">The second url.</param>
        /// <returns>The first response result.</returns>
        public static async Task<string> FirstRespondingUrl(string urlA, string urlB)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                Task<string> taskA = httpClient.GetStringAsync(urlA);
                Task<string> taskB = httpClient.GetStringAsync(urlB);

                Task<string> firstRespondTask = await Task.WhenAny(taskA, taskB);

                return await firstRespondTask;
            }
        }
        
        #endregion

        #region Execute actions after all tasks

        /// <summary>
        /// Return the given number after waiting for some time.
        /// </summary>
        /// <param name="delay">The delay and value to return.</param>
        /// <returns>The provided value.</returns>
        static async Task<int> DelayAndReturnAsync(int delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            return delay;
        }

        /// <summary>
        /// Wait for each task individually.
        /// </summary>
        /// <returns></returns>
        public static async Task ProcessTasksV1()
        {
            // create the sequence of tasks
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);

            var tasks = ImmutableList.Create(taskA, taskB, taskC);

            foreach (var task in tasks)
            {
                var result = await task;
                Console.WriteLine(result);
            }
        }

        static async Task AwaitAndProcessAsync(Task<int> task)
        {
            var result = await task;
            Console.WriteLine(result);
        }

        /// <summary>
        /// This second solution creates a wrapper around the original task adding the post processing step.
        /// </summary>
        /// <returns></returns>
        public static async Task ProcessTasksV2()
        {
            // create the sequence of tasks
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);

            var tasks = new[] {taskA, taskB, taskC};

            await Task.WhenAll(from t in tasks select AwaitAndProcessAsync(t));
        }

        /// <summary>
        /// This third solution uses an extension method that order tasks by completion.
        /// </summary>
        /// <returns></returns>
        public static async Task ProcessTasksV3()
        {
            // create the sequence of tasks
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);

            var tasks = new[] {taskA, taskB, taskC};

            foreach (var task in tasks.OrderByCompletion())
            {
                var result = await task;
                Console.WriteLine(result);
            }
        }

        #endregion

        #region Context Handling

        static async Task ResumeOnContextAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            // this method resumes within the same context.
        }

        static async Task ResumeWithoutContextAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            
            // This method discards its context when it resumes.
        }

        #endregion
        
    }
}