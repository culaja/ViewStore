using ViewStore.Abstractions;

namespace ViewStore.Cache
{
    internal sealed class OutgoingCache
    {
        private readonly object _sync = new();
        private CachedItems _currentCache = new();
        private CachedItems _drainedCache = new();

        public void AddOrUpdate(ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                _currentCache.AddOrUpdate(viewEnvelope);
            }
        }

        public void Remove(string viewId, GlobalVersion globalVersion)
        {
            lock (_sync)
            {
                _currentCache.Remove(viewId, globalVersion);
            }
        }

        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope, out bool isDeleted)
        {
            lock (_sync)
            {
                return _currentCache.TryGetValue(viewId, out viewEnvelope, out isDeleted) || 
                       (isDeleted == false && _drainedCache.TryGetValue(viewId, out viewEnvelope, out isDeleted));
            }
        }

        public GlobalVersion? LastGlobalVersion()
        {
            lock (_sync)
            {
                return GlobalVersion.Max(
                    _currentCache.LastGlobalVersion(),
                    _drainedCache.LastGlobalVersion());
            }
        }

        public CachedItems Renew()
        {
            lock (_sync)
            {
                _drainedCache = _currentCache;
                _currentCache = new();
                return _drainedCache;
            }
        }
    }
}