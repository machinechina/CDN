using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CDN.Infrastructure
{
    public class IntervalTask
    {
        public static Task Start(
             TimeSpan pollInterval,
             Action action,
             CancellationToken token)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                () =>
                {
                    for (;;)
                    {
                        if (token.WaitCancellationRequested(pollInterval))
                            break;
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            ApplicationHelper.Log(ex);
                        }
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    internal static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}