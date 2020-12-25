using System;

namespace Stores.MongoDb
{
    internal sealed class MongoDbWrongMatchedCountException : Exception
    {
        public MongoDbWrongMatchedCountException(string entityId, long matchedCount)
            : base($"Matched count is '{matchedCount}' but should be 1 for entity with id '{entityId}'.")
        {
        }
    }
}