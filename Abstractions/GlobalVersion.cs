using System;

namespace ViewStore.Abstractions
{
    public readonly struct GlobalVersion : IComparable<GlobalVersion>
    {
        public long Value { get; }

        public GlobalVersion(long value)
        {
            Value = value;
        }

        public static readonly GlobalVersion Start = new(0L);
        public static GlobalVersion Of(long value) => new(value);

        public static GlobalVersion? Max(GlobalVersion? a, GlobalVersion? b)
        {
            if (a == null)
            {
                return b;
            }

            if (b == null)
            {
                return a;
            }

            return a.Value > b.Value ? a : b;
        }
        
        public static bool operator <(GlobalVersion p1, GlobalVersion p2)
        {
            return p1.Value < p2.Value;
        }
        
        public static bool operator >(GlobalVersion p1, GlobalVersion p2)
        {
            return p1.Value > p2.Value;
        }
        
        public static bool operator >=(GlobalVersion p1, GlobalVersion p2) => p1 > p2 || p1 == p2;
        
        public static bool operator <=(GlobalVersion p1, GlobalVersion p2) => p1 < p2 || p1 == p2;
        
        public static bool operator ==(GlobalVersion p1, GlobalVersion p2) => 
            p1.Value == p2.Value;
        
        public static bool operator !=(GlobalVersion p1, GlobalVersion p2) => !(p1 == p2);
        
        public override bool Equals(object? obj) => 
            obj is GlobalVersion && Equals((GlobalVersion)obj);

        public bool Equals(GlobalVersion other) => this == other;

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode()*397);
            }
        }
        
        public override string ToString() => $"GP:{Value}";

        public int CompareTo(GlobalVersion other) => Value.CompareTo(other.Value);
    }
}