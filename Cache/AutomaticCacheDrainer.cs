using System;
using System.Threading;

namespace Cache
{
    public delegate void OnSendingExceptionDelegate(Exception exception);
    public delegate void OnDrainFinishedDelegate(int count);
    
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
            _drainPeriod = drainPeriod;
            _worker = new Thread(Work);
            _worker.Start();
        }

        private void Work()
        {
            Thread.Sleep(_drainPeriod);
            while (!_isStopRequested || _manualCacheDrainer.ItemsToDrain != 0)
            {
                TryDrainCache();
            }
        }

        private void TryDrainCache()
        {
            try
            {
                var itemsToDrain = _manualCacheDrainer.ItemsToDrain;
                _manualCacheDrainer.DrainCache();
                OnDrainFinishedEvent?.Invoke(itemsToDrain);
            }
            catch (Exception e)
            {
                OnSendingExceptionEvent?.Invoke(e);
            }
        }

        public void Dispose()
        {
            _isStopRequested = true;
            _worker.Join();
        }
    }
}