using System;

namespace Stores.MongoDb
{
    internal sealed class MongoDbWrongMatchedCountException : Exception
    {
        public MongoDbWrongMatchedCountException(string viewId, long matchedCount)
            : base($"Matched count is '{matchedCount}' but should be 1 for view with id '{viewId}'.")
        {
        }
    }
}