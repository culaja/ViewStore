using System;
using System.Threading;

namespace ViewStore.Cache;

internal delegate void OnSendingExceptionDelegate(Exception exception);

internal delegate void OnDrainFinishedDelegate(DrainStatistics drainStatistics);
    
internal sealed class AutomaticCacheDrainer : IDisposable
{
    private readonly ManualCacheDrainer _manualCacheDrainer;
    private readonly TimeSpan _drainPeriod;
    private readonly Thread _worker;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event OnSendingExceptionDelegate? OnSendingExceptionEvent;
    public event OnDrainFinishedDelegate? OnDrainFinishedEvent;

    public AutomaticCacheDrainer(
        ManualCacheDrainer manualCacheDrainer,
        TimeSpan drainPeriod,
        bool isBackgroundWorker)
    {
        _manualCacheDrainer = manualCacheDrainer;
        manualCacheDrainer.OnSendingExceptionEvent += exception => OnSendingExceptionEvent?.Invoke(exception);
        manualCacheDrainer.OnDrainFinishedEvent += views => OnDrainFinishedEvent?.Invoke(views);
        _drainPeriod = drainPeriod;
        _worker = new Thread(Work) { IsBackground = isBackgroundWorker };
        _worker.Start();
    }

    private void Work()
    {
        while (true)
        {
            Sleep();
                
            var drainedItems = _manualCacheDrainer.DrainCacheUntilEmpty();
            if (_cancellationTokenSource.IsCancellationRequested && drainedItems == 0)
            {
                break;
            }
        }
    }

    private void Sleep()
    {
        _cancellationTokenSource.Token.WaitHandle.WaitOne(_drainPeriod);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _worker.Join();
    }
}