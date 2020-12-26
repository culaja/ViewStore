using System;
using System.Collections.Generic;
using System.Threading;
using Abstractions;

namespace Cache
{
    public delegate void OnSendingExceptionDelegate(Exception exception);

    public delegate void OnDrainFinishedDelegate(IReadOnlyList<IView> views);
    
    public sealed class AutomaticCacheDrainer : IDisposable
    {
        private readonly ManualCacheDrainer _manualCacheDrainer;
        private readonly TimeSpan _drainPeriod;
        private readonly Thread _worker;
        private bool _isStopRequested;

        public event OnSendingExceptionDelegate? OnSendingExceptionEvent;
        public event OnDrainFinishedDelegate? OnDrainFinishedEvent;

        public AutomaticCacheDrainer(
            ManualCacheDrainer manualCacheDrainer,
            TimeSpan drainPeriod
            )
        {
            _manualCacheDrainer = manualCacheDrainer;
            manualCacheDrainer.OnSendingExceptionEvent += exception => OnSendingExceptionEvent?.Invoke(exception);
            manualCacheDrainer.OnDrainFinishedEvent += views => OnDrainFinishedEvent?.Invoke(views);
            _drainPeriod = drainPeriod;
            _worker = new Thread(Work);
            _worker.Start();
        }

        private void Work()
        {
            while (true)
            {
                Thread.Sleep(_drainPeriod);
                
                var drainedItems = _manualCacheDrainer.TryDrainCache();
                if (_isStopRequested && drainedItems == 0)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            _isStopRequested = true;
            _worker.Join();
        }
    }
}