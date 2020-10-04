using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.Logging;

namespace RunThis.Tests
{
    public static class TaskExtensions
    {

        public static void Dismiss<TLogger>(this Task task, TLogger logger = default, int timeout = 0)
            where TLogger : class, ILogger
        {
            if (!task.IsCompletedSuccessfully && (logger != null || timeout != 0))
            {
                if (timeout != 0)
                    task = task.TimeoutAfter(timeout);

                task.ContinueWith(x =>
                {
                    if (!x.IsFaulted)
                        return;

                    logger?.LogError(x.Exception, "Dismissed task failed (Task)");
                });
            }
        }

        public static void Dismiss<TResult, TLogger>(this Task<TResult> task, TLogger logger = default, int timeout = 0)
            where TLogger : class, ILogger
        {
            if (!task.IsCompletedSuccessfully && (logger != null || timeout != 0))
            {
                if (timeout != 0)
                    task = task.TimeoutAfter(timeout);

                task.ContinueWith(x =>
                {
                    if (!x.IsFaulted)
                        return;

                    logger?.LogError(x.Exception, $"Dismissed task failed (Task<{typeof(TResult).Name}>)");
                });
            }
        }


        public static void Dismiss<TLogger>(this ValueTask valueTask, TLogger logger = default, int timeout = 0)
            where TLogger : class, ILogger
        {
            if (!valueTask.IsCompletedSuccessfully && (logger != null || timeout != 0))
            {
                var task = valueTask.AsTask();
                if (timeout != 0)
                    task = task.TimeoutAfter(timeout);

                task.ContinueWith(x =>
                {
                    if (!x.IsFaulted)
                        return;

                    logger.LogError(x.Exception, $"Dismissed task failed (ValueTask)");
                });

            }
        }

        public static void Dismiss<TResult, TLogger>(this ValueTask<TResult> valueTask, TLogger logger = default, int timeout = 0)
            where TLogger : class, ILogger
        {
            if (!valueTask.IsCompletedSuccessfully && (logger != null || timeout != 0))
            {
                var task = valueTask.AsTask();
                if (timeout != 0)
                    task = task.TimeoutAfter(timeout);

                task.ContinueWith(x =>
                {
                    if (!x.IsFaulted)
                        return;

                    logger.LogError(x.Exception, $"Dismissed task failed (ValueTask<{typeof(TResult).Name}>)");
                });

            }
        }




        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout, CancellationToken ct = default)
        {
            if (task.IsCompletedSuccessfully)
                return;

            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsTimeout, ct));
            if (completedTask != task)
                throw new TimeoutException($"Task exceeded {millisecondsTimeout}ms timeout");

            task.Wait();
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, int millisecondsTimeout, CancellationToken ct = default)
        {
            if (task.IsCompletedSuccessfully)
                return task.Result;

            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsTimeout, ct));
            if (completedTask == task)
                return task.Result;
            throw new TimeoutException($"Task<{typeof(T).Name}> exceeded {millisecondsTimeout}ms timeout");
        }

        public static async Task TimeoutAfter(this ValueTask valueTask, int millisecondsTimeout, CancellationToken ct = default)
        {
            if (valueTask.IsCompletedSuccessfully)
                return;

            var task = valueTask.AsTask();

            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsTimeout, ct));
            if (completedTask != task)
                throw new TimeoutException($"ValueTask exceeded {millisecondsTimeout}ms timeout");

            task.Wait();
        }

        public static async Task<T> TimeoutAfter<T>(this ValueTask<T> valueTask, int millisecondsTimeout, CancellationToken ct = default)
        {
            if (valueTask.IsCompletedSuccessfully)
                return valueTask.Result;

            var task = valueTask.AsTask();
            var completedTask = await Task.WhenAny(task, Task.Delay(millisecondsTimeout, ct));
            if (completedTask == task)
                return task.Result;
            throw new TimeoutException($"ValueTask<{typeof(T).Name}> exceeded {millisecondsTimeout}ms timeout");
        }



    }
}
