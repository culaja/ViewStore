using System;

namespace ViewStore.Abstractions
{
    public readonly struct TrackedPosition : IEquatable<TrackedPosition>, IComparable<TrackedPosition>
    {
        public ulong Part1 { get; }
        public ulong Part2 { get; }

        public TrackedPosition(ulong part1, ulong part2)
        {
            Part1 = part1;
            Part2 = part2;
        }

        public static TrackedPosition Of(ulong part1) => new(part1, 0UL);
        public static TrackedPosition Of(ulong part1, ulong part2) => new(part1, part2);
        
        public static bool operator <(TrackedPosition p1, TrackedPosition p2)
        {
            if (p1.Part1 < p2.Part1)
                return true;
            return (long) p1.Part1 == (long) p2.Part1 && p1.Part2 < p2.Part2;
        }
        
        public static bool operator >(TrackedPosition p1, TrackedPosition p2)
        {
            if (p1.Part1 > p2.Part1)
                return true;
            return (long) p1.Part1 == (long) p2.Part1 && p1.Part2 > p2.Part2;
        }
        
        public static bool operator >=(TrackedPosition p1, TrackedPosition p2) => p1 > p2 || p1 == p2;
        
        public static bool operator <=(TrackedPosition p1, TrackedPosition p2) => p1 < p2 || p1 == p2;
        
        public static bool operator ==(TrackedPosition p1, TrackedPosition p2) => Equals(p1, p2);
        
        public static bool operator !=(TrackedPosition p1, TrackedPosition p2) => !(p1 == p2);
        
        public int CompareTo(TrackedPosition other)
        {
            if (this == other)
                return 0;
            return !(this > other) ? -1 : 1;
        }
        
        public override bool Equals(object? obj) => obj is TrackedPosition other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Part1, Part2);

        public bool Equals(TrackedPosition other) => (long) Part1 == (long) other.Part1 && (long) Part2 == (long) other.Part2;
        
        public override string ToString() => $"P1:{Part1}/P2:{Part2}";

    }
}