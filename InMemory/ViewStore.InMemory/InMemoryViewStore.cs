﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViewStore.Abstractions;

namespace ViewStore.InMemory
{
    public sealed class InMemoryViewStore : IViewStore
    {
        private const string LastDeletedViewId = "LastDeleted-b24ae98262724d27bd8e31c34ff11f1a";
        
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly Dictionary<string, ViewEnvelope> _dictionary = new();

        public GlobalVersion? ReadLastGlobalVersion()
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.Count > 0
                    ? _dictionary.Values.Max(v => v.GlobalVersion)
                    : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<GlobalVersion?> ReadLastGlobalVersionAsync() => Task.FromResult(ReadLastGlobalVersion());

        public ViewEnvelope? Read(string viewId)
        {
            _lock.EnterReadLock();
            try
            {
                if (_dictionary.TryGetValue(viewId, out var view))
                {
                    return view;
                }

                return default;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<ViewEnvelope?> ReadAsync(string viewId) => Task.FromResult(Read(viewId));

        public void Save(ViewEnvelope viewEnvelope)
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary[viewEnvelope.Id] = viewEnvelope;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            Save(viewEnvelope);
            return Task.CompletedTask;
        }

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            foreach (var viewEnvelope in viewEnvelopes)
            {
                Save(viewEnvelope);
            }
        }

        public async Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            foreach (var viewEnvelope in viewEnvelopes)
            {
                await SaveAsync(viewEnvelope);
            }
        }

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary.Remove(viewId);
                _dictionary[LastDeletedViewId] = ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Task DeleteAsync(string viewId, GlobalVersion globalVersion)
        {
            Delete(viewId, globalVersion);
            return Task.CompletedTask;
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            foreach (var viewId in viewIds)
            {
                Delete(viewId, globalVersion);
            }
        }

        public async Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            foreach (var viewId in viewIds)
            {
                await DeleteAsync(viewId, globalVersion);
            }
        }
    }
}