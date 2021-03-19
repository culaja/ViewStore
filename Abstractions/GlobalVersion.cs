namespace ViewStore.Abstractions
{
    public readonly struct GlobalVersion
    {
        public ulong Part1 { get; }
        public ulong Part2 { get; }

        public GlobalVersion(ulong part1, ulong part2)
        {
            Part1 = part1;
            Part2 = part2;
        }

        public static GlobalVersion Of(ulong part1) => new(part1, 0UL);
        public static GlobalVersion Of(ulong part1, ulong part2) => new(part1, part2);
        
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

    }
}