using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SEOP.Framework.Config;

namespace CDN.Infrastructure
{
    public abstract class Worker : IWorker
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _messageDispatcherWorker;
        private Int32 _loopInterval;
        public Worker(Int32 loopInterval)
        {
            _loopInterval = loopInterval;
        }

        public virtual void Start()
        {
            lock (this)
            {
                if (IsRunning) return;
                IsRunning = true;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            if (_loopInterval >= 0)
            {
                _messageDispatcherWorker = IntervalTask.Start(TimeSpan.FromMilliseconds(_loopInterval),
                         DoWork, _cancellationTokenSource.Token);
            }
            else
            {
                _messageDispatcherWorker = new Task(DoWork);
                _messageDispatcherWorker.Start();
            }
        }

        public virtual void Stop()
        {
            lock (this)
            {
                if (!IsRunning) return;

                _cancellationTokenSource.Cancel();
                _messageDispatcherWorker.Wait();

                IsRunning = false;
            }
        }

        protected void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected abstract void DoWork();

        public void Wait()
        {
            _messageDispatcherWorker.Wait();
            IsRunning = false;
        }

        protected bool IsCancellationRequested
        {
            get
            {
                return _cancellationTokenSource.IsCancellationRequested;
            }
        }

        public bool IsRunning
        {
            get; protected set;
        } = false;
    }
}
