using ViewStore.Abstractions;

namespace ViewStore.WriteBehindCache
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

        public void Remove(ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                _currentCache.Remove(viewEnvelope);
            }
        }

        public bool TryGetValue(string viewId, out ViewEnvelope viewEnvelope)
        {
            lock (_sync)
            {
                return _currentCache.TryGetValue(viewId, out viewEnvelope) || _drainedCache.TryGetValue(viewId, out viewEnvelope);
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