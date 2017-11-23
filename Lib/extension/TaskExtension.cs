using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.extension
{
    public static class TaskExtension
    {
        public static void WaitOneOrThrow(this ManualResetEvent reset_event, TimeSpan span, string msg = null)
        {
            if (!reset_event.WaitOne(span))
            {
                throw new Exception(msg ?? "等待信号量超时");
            }
        }

        public static void WaitOneOrThrow(this AutoResetEvent reset_event, TimeSpan span, string msg = null)
        {
            if (!reset_event.WaitOne(span))
            {
                throw new Exception(msg ?? "等待信号量超时");
            }
        }

        public static void WaitOneOrThrow(this ManualResetEventSlim reset_event, TimeSpan span, string msg = null)
        {
            if (!reset_event.Wait(span))
            {
                throw new Exception(msg ?? "等待信号量超时");
            }
        }

        /// <summary>https://msdn.microsoft.com/zh-cn/library/hh873178(v=vs.110).aspx </summary>
        public static IAsyncResult AsApm<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.AsyncState == state)
            {
                if (callback != null)
                    task.ContinueWith(t => callback(t), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

                return task;
            }

            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(t.Result);

                callback?.Invoke(tcs.Task);
            }, TaskScheduler.Default);

            return tcs.Task;
        }

        /// <summary>WaitHandle转为Task。反转：WaitHandle wh = ((IAsyncResult)task).AsyncWaitHandle;</summary>
        /// <returns>Task</returns>
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle)
        {
            if (waitHandle == null)
                throw new ArgumentNullException(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle, (state, timedOut) => tcs.TrySetResult(true), null, -1, true);
            var t = tcs.Task;
            t.ContinueWith(_ => rwh.Unregister(null));
            return t;
        }

        #region Timeout<T>
        public static async Task<T> Timeout<T>(this Task<T> task, TimeSpan delay)
        {
            if (await Task.WhenAny(task, Task.Delay(delay)) == task)
                return await task;

            throw new TimeoutException();
        }

        public static async Task<T> Timeout<T>(this Task<T> task, TimeSpan delay, CancellationToken token)
        {
            if (await Task.WhenAny(task, Task.Delay(delay, token)) == task)
                return await task;

            throw new TimeoutException();
        }

        public static async Task<T> Timeout<T>(this Task<T> task, int millisecondsDelay)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsDelay)) == task)
                return await task;

            throw new TimeoutException();
        }

        public static async Task<T> Timeout<T>(this Task<T> task, int millisecondsDelay, CancellationToken token)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsDelay, token)) == task)
                return await task;

            throw new TimeoutException();
        }
        #endregion

        #region Timeout
        public static async Task Timeout(this Task task, TimeSpan delay)
        {
            if (await Task.WhenAny(task, Task.Delay(delay)) == task)
                await task;
            else
                throw new TimeoutException();
        }

        public static async Task Timeout(this Task task, TimeSpan delay, CancellationToken token)
        {
            if (await Task.WhenAny(task, Task.Delay(delay, token)) == task)
                await task;
            else
                throw new TimeoutException();
        }

        public static async Task Timeout(this Task task, int millisecondsDelay)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsDelay)) == task)
                await task;
            else
                throw new TimeoutException();
        }

        public static async Task Timeout(this Task task, int millisecondsDelay, CancellationToken token)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsDelay, token)) == task)
                await task;
            else
                throw new TimeoutException();
        }
        #endregion

        #region GetAwaiter
        //https://blogs.msdn.microsoft.com/pfxteam/2011/01/13/await-anything/
        //https://blogs.msdn.microsoft.com/pfxteam/2012/04/12/asyncawait-faq/
        public static TaskAwaiter<int> GetAwaiter(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

            if (process.HasExited)
                tcs.TrySetResult(process.ExitCode);

            return tcs.Task.GetAwaiter();
        }

        /// <summary>
        /// Gets the awaiter.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>TaskAwaiter.</returns>
        public static TaskAwaiter<bool> GetAwaiter(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (cancellationToken.IsCancellationRequested)
                tcs.SetResult(true);
            else
                cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);

            return tcs.Task.GetAwaiter();
        }
        #endregion

        #region ParallelInvoke

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="maxInvoke">最大并行执行量，≤1时为CPU核心数量</param>
        [Obsolete("请使用" + nameof(ParallelSelect) + "<TSource, TResult>(Func<TSource, Task<TResult>> selector, int maxInvoke = 0)", true)]
        public static IEnumerable<Task<T>> ParallelInvoke<T>(this IEnumerable<Func<Task<T>>> funcs, int maxInvoke = 0)
        {
            if (maxInvoke <= 1)
                maxInvoke = Environment.ProcessorCount;

            var semaphore = new SemaphoreSlim(maxInvoke, maxInvoke);

            return funcs.Select(func => ParallelInvoke(semaphore, func));
        }

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="maxInvoke">最大并行执行量，≤1时为CPU核心数量</param>
        [Obsolete("请使用" + nameof(ParallelSelect) + "<TSource, TResult>(Func<TSource, Task> selector, int maxInvoke = 0)", true)]
        public static IEnumerable<Task> ParallelInvoke(this IEnumerable<Func<Task>> funcs, int maxInvoke = 0)
        {
            if (maxInvoke <= 1)
                maxInvoke = Environment.ProcessorCount;

            var semaphore = new SemaphoreSlim(maxInvoke, maxInvoke);

            return funcs.Select(func => ParallelInvoke(semaphore, func));
        }

        private static async Task<T> ParallelInvoke<T>(SemaphoreSlim semaphore, Func<Task<T>> func)
        {
            await semaphore.WaitAsync();
            try
            {
                return await func();
            }
            finally
            {
                semaphore.Release();
            }
        }
        private static async Task ParallelInvoke(SemaphoreSlim semaphore, Func<Task> func)
        {
            await semaphore.WaitAsync();
            try
            {
                await func();
            }
            finally
            {
                semaphore.Release();
            }
        }
        #endregion

        #region ParallelSelect

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量</param>
        public static IEnumerable<Task<TResult>> ParallelSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector, int maxParallel = 0) =>
            ParallelSelect(source, selector, CancellationToken.None, maxParallel);

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="token">The token.</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量</param>
        public static IEnumerable<Task<TResult>> ParallelSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector, CancellationToken token, int maxParallel = 0)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var invoker = new ParallelInvoker(maxParallel, token);
                while (enumerator.MoveNext())
                {
                    yield return invoker.Invoke(enumerator.Current, selector);
                }
            }
        }

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量</param>
        /// <remarks>不要对已经开始的Task进行限制，比如List&lt;Task&gt;，因为Task已经开始了。</remarks>
        public static IEnumerable<Task> ParallelSelect<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> selector, int maxParallel = 0) =>
            ParallelSelect(source, selector, CancellationToken.None, maxParallel);

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="token">The token.</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量</param>
        /// <remarks>不要对已经开始的Task进行限制，比如List&lt;Task&gt;，因为Task已经开始了。</remarks>
        public static IEnumerable<Task> ParallelSelect<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> selector, CancellationToken token, int maxParallel = 0)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var invoker = new ParallelInvoker(maxParallel, token);
                while (enumerator.MoveNext())
                {
                    yield return invoker.Invoke(enumerator.Current, selector);
                }
            }
        }

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量。</param>
        public static IEnumerable<Task<TResult>> ParallelSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, Task<TResult>> selector, int maxParallel = 0) =>
            ParallelSelect(source, selector, CancellationToken.None, maxParallel);

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="token">The token.</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量。</param>
        public static IEnumerable<Task<TResult>> ParallelSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, Task<TResult>> selector, CancellationToken token, int maxParallel = 0)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var index = 0;
                var invoker = new ParallelInvoker(maxParallel, token);
                while (enumerator.MoveNext())
                {
                    yield return invoker.Invoke(enumerator.Current, index++, selector);
                }
            }
        }

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量</param>
        public static IEnumerable<Task> ParallelSelect<TSource>(this IEnumerable<TSource> source, Func<TSource, int, Task> selector, int maxParallel = 0) =>
            ParallelSelect(source, selector, CancellationToken.None, maxParallel);

        /// <summary>使用信号号方式限制异步方法并发量</summary>
        /// <param name="source">一个值序列，要对该序列调用转换函数。</param>
        /// <param name="selector">应用于每个元素的转换函数。</param>
        /// <param name="token">The token.</param>
        /// <param name="maxParallel">最大并行执行量，≤1时为CPU核心数量</param>
        public static IEnumerable<Task> ParallelSelect<TSource>(this IEnumerable<TSource> source, Func<TSource, int, Task> selector, CancellationToken token, int maxParallel = 0)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var index = 0;
                var invoker = new ParallelInvoker(maxParallel, token);
                while (enumerator.MoveNext())
                {
                    yield return invoker.Invoke(enumerator.Current, index++, selector);
                }
            }
        }

        private class ParallelInvoker
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly CancellationToken _token;

            public ParallelInvoker(int maxParallel) : this(maxParallel, CancellationToken.None) { }

            public ParallelInvoker(int maxParallel, CancellationToken token)
            {
                if (maxParallel < 2)
                    maxParallel = Environment.ProcessorCount;

                _semaphore = new SemaphoreSlim(maxParallel, maxParallel);
                _token = token;
            }

            public async Task<TResult> Invoke<TSource, TResult>(TSource source, Func<TSource, Task<TResult>> selector)
            {
                await _semaphore.WaitAsync(_token);
                try
                {
                    return await selector(source);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            public async Task Invoke<T>(T source, Func<T, Task> selector)
            {
                await _semaphore.WaitAsync(_token);
                try
                {
                    await selector(source);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            public async Task<TResult> Invoke<TSource, TResult>(TSource source, int index, Func<TSource, int, Task<TResult>> selector)
            {
                await _semaphore.WaitAsync(_token);
                try
                {
                    return await selector(source, index);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            public async Task Invoke<T>(T source, int index, Func<T, int, Task> selector)
            {
                await _semaphore.WaitAsync(_token);
                try
                {
                    await selector(source, index);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
        #endregion
    }
    
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> _mTcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() => _mTcs.Task;

        public void Set()
        {
            var tcs = _mTcs;
            Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true),
                tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            tcs.Task.Wait();
        }

        public void Reset()
        {
            while (true)
            {
                var tcs = _mTcs;
                if (!tcs.Task.IsCompleted ||
#pragma warning disable 420
                    Interlocked.CompareExchange(ref _mTcs,
                    new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
#pragma warning restore 420
            }
        }
    }
}
