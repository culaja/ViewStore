using System;

namespace ViewStore.Abstractions
{
    public readonly struct GlobalVersion : IComparable<GlobalVersion>
    {
        public long Part1 { get; }
        public long Part2 { get; }

        public GlobalVersion(long part1, long part2)
        {
            Part1 = part1;
            Part2 = part2;
        }

        public static readonly GlobalVersion Start = new(0L, 0L);
        public static GlobalVersion Of(long part1) => new(part1, 0L);
        public static GlobalVersion Of(long part1, long part2) => new(part1, part2);
        
        public static bool operator <(GlobalVersion p1, GlobalVersion p2)
        {
            return p1.Part1 < p2.Part1 || (p1.Part1 == p2.Part1 && p1.Part2 < p2.Part2);
        }
        
        public static bool operator >(GlobalVersion p1, GlobalVersion p2)
        {
            return p1.Part1 > p2.Part1 || (p1.Part1 == p2.Part1 && p1.Part2 > p2.Part2);
        }
        
        public static bool operator >=(GlobalVersion p1, GlobalVersion p2) => p1 > p2 || p1 == p2;
        
        public static bool operator <=(GlobalVersion p1, GlobalVersion p2) => p1 < p2 || p1 == p2;
        
        public static bool operator ==(GlobalVersion p1, GlobalVersion p2) => 
            p1.Part1 == p2.Part1 && p1.Part2 == p2.Part2;
        
        public static bool operator !=(GlobalVersion p1, GlobalVersion p2) => !(p1 == p2);
        
        public override bool Equals(object obj) => 
            obj is GlobalVersion && Equals((GlobalVersion)obj);

        public bool Equals(GlobalVersion other) => this == other;

        public override int GetHashCode()
        {
            unchecked
            {
                return (Part1.GetHashCode()*397) ^ Part2.GetHashCode();
            }
        }
        
        public override string ToString() => $"P1:{Part1}/P2:{Part2}";

        public int CompareTo(GlobalVersion other)
        {
            var part1Comparison = Part1.CompareTo(other.Part1);
            if (part1Comparison != 0) return part1Comparison;
            return Part2.CompareTo(other.Part2);
        }
    }
}