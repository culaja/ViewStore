﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewStore.Abstractions
{
    public interface IViewStore
    {
        GlobalVersion? ReadLastGlobalVersion();
        Task<GlobalVersion?> ReadLastGlobalVersionAsync();
        
        ViewEnvelope? Read(string viewId);
        Task<ViewEnvelope?> ReadAsync(string viewId);

        void Save(ViewEnvelope viewEnvelope);
        Task SaveAsync(ViewEnvelope viewEnvelope);

        void Save(IEnumerable<ViewEnvelope> viewEnvelopes);
        Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes);

        void Delete(ViewEnvelope viewEnvelope);
        Task DeleteAsync(ViewEnvelope viewEnvelope);
        
        void Delete(IEnumerable<ViewEnvelope> viewEnvelopes);
        Task DeleteAsync(IEnumerable<ViewEnvelope> viewEnvelopes);
    }
}