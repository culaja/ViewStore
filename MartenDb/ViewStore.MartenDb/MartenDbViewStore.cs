using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using ViewStore.Abstractions;

namespace ViewStore.MartenDb
{
    internal sealed class MartenDbViewStore : IViewStore
    {
        private const string LastDeletedViewId = "LastDeleted-b24ae98262724d27bd8e31c34ff11f1a";
        
        private readonly IDocumentStore _documentStore;

        public MartenDbViewStore(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }
        
        public GlobalVersion? ReadLastGlobalVersion()
        {
            using var session = _documentStore.OpenSession();
            var globalVersion = session
                .Query<ViewEnvelopeInternal>()
                .OrderByDescending(ve => ve.GlobalVersion)
                .FirstOrDefault()
                ?.GlobalVersion;
            return globalVersion != null ? GlobalVersion.Of(globalVersion.Value) : null;
        }

        public async Task<GlobalVersion?> ReadLastGlobalVersionAsync()
        {
            using var session =  _documentStore.OpenSession();
            var viewEnvelopeInternal = await session
                .Query<ViewEnvelopeInternal>()
                .OrderByDescending(ve => ve.GlobalVersion)
                .FirstOrDefaultAsync();
            return viewEnvelopeInternal != null ? GlobalVersion.Of(viewEnvelopeInternal.GlobalVersion) : null;
        }

        public ViewEnvelope? Read(string viewId)
        {
            using var session = _documentStore.OpenSession();
            return session
                .Query<ViewEnvelopeInternal>()
                .SingleOrDefault(ve => ve.Id == viewId);
        }

        public async Task<ViewEnvelope?> ReadAsync(string viewId)
        {
            using var session = _documentStore.OpenSession();
            var viewEnvelope = await session
                .Query<ViewEnvelopeInternal>()
                .SingleOrDefaultAsync(ve => ve.Id == viewId);
            return viewEnvelope;
        }

        public void Save(ViewEnvelope viewEnvelope)
        {
            using var session = _documentStore.OpenSession();
            session.Store(viewEnvelope.ToViewEnvelopeInternal());
            session.SaveChanges();
        }

        public async Task SaveAsync(ViewEnvelope viewEnvelope)
        {
            using var session = _documentStore.OpenSession();
            session.Store(viewEnvelope.ToViewEnvelopeInternal());
            await session.SaveChangesAsync();
        }

        public void Save(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var session = _documentStore.OpenSession();
            session.StoreObjects(viewEnvelopes.Select(ve => ve.ToViewEnvelopeInternal()));
            session.SaveChanges();
        }
        
        public async Task SaveAsync(IEnumerable<ViewEnvelope> viewEnvelopes)
        {
            using var session = _documentStore.OpenSession();
            session.StoreObjects(viewEnvelopes.Select(ve => ve.ToViewEnvelopeInternal()));
            await session.SaveChangesAsync();
        }

        public void Delete(string viewId, GlobalVersion globalVersion)
        {
            using var session = _documentStore.OpenSession();
            session.DeleteWhere<ViewEnvelopeInternal>(ve => ve.Id == viewId);
            session.Store(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            session.SaveChanges();
        }

        public async Task DeleteAsync(string viewId, GlobalVersion globalVersion)
        {
            using var session = _documentStore.OpenSession();
            session.DeleteWhere<ViewEnvelopeInternal>(ve => ve.Id == viewId);
            session.Store(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            await session.SaveChangesAsync();
        }

        public void Delete(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            using var session = _documentStore.OpenSession();
            foreach (var viewId in viewIds) session.DeleteWhere<ViewEnvelopeInternal>(ve => ve.Id == viewId);
            session.Store(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            session.SaveChanges();
        }

        public async Task DeleteAsync(IEnumerable<string> viewIds, GlobalVersion globalVersion)
        {
            using var session = _documentStore.OpenSession();
            foreach (var viewId in viewIds) session.DeleteWhere<ViewEnvelopeInternal>(ve => ve.Id == viewId);
            session.Store(ViewEnvelope.EmptyWith(LastDeletedViewId, globalVersion));
            await session.SaveChangesAsync();
        }
    }
}