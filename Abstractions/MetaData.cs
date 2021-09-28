using System.Collections.Generic;

namespace ViewStore.Abstractions
{
    public sealed class MetaData
    {
        public Dictionary<string, string> KeyValues { get; }

        public MetaData(Dictionary<string, string> keyValues)
        {
            KeyValues = keyValues;
        }

        public static MetaData New() => new MetaData(new Dictionary<string, string>());
        
        public void Set(string key, string value) => KeyValues[key] = value;
        public string? Get(string key) => KeyValues.TryGetValue(key, out var value) ? value : null;
    }
}