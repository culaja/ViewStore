using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using ViewStore.Abstractions;

namespace ViewStore.MartenDb
{
    internal sealed class MartenDbViewStore : IViewStore
    {
        private readonly IDocumentStore _documentStore;

        public MartenDbViewStore(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public GlobalVersion? ReadLastGlobalVersion()
        {
            using var session = _documentStore.OpenSession();
            return session
                .Query<ViewEnvelope>()
                .OrderByDescending(ve => ve.GlobalVersion)
                .FirstOrDefault()
                ?.GlobalVersion;
        }

        public async Task<GlobalVersion?> ReadLastGlobalVersionAsync()
        {
            using var session =  _documentStore.OpenSession();
            var lastUpdatedView = await session
                .Query<ViewEnvelope>()
                .OrderByDescending(ve => ve.GlobalVersion)
                .FirstOrDefaultAsync();
            return lastUpdatedView?.GlobalVersion;
        }

        public ViewEnvelope? Read(string viewId)
        {
            using var session = _documentStore.OpenSession();
            return session
                .Query<ViewEnvelope>()
                .SingleOrDefault(ve => ve.Id == viewId);
        }

        public async Task<ViewEnvelope?> ReadAsync(string viewId)
        {
            using var session = _documentStore.OpenSession();
            var viewEnvelope = await session
                .Query<ViewEnvelope>()
                .SingleOrDefaultAsync(ve => ve.Id == viewId);
            return viewEnvelope;
        }

        public void Save(ViewEnvelope viewEnvelope)
        {
            using var session = _documentStore.OpenSession();
            session.Store(viewEnvelope);
            session.SaveChanges();
        }

        public async Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            using var session = _documentStore.OpenSession();
            session.Store(viewEnvelope);
            await session.SaveChangesAsync();
        }

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var session = _documentStore.OpenSession();
            session.StoreObjects(viewEnvelopes);
            session.SaveChanges();
        }

        public async Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var session = _documentStore.OpenSession();
            session.StoreObjects(viewEnvelopes);
            await session.SaveChangesAsync();
        }
    }
}