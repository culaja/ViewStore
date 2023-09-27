using System;

namespace ViewStore.Abstractions
{
    public sealed class TestView : IView
    {
        public static readonly ViewRecord TestViewEnvelope1 = new("1", new TestView(1), 1L);
        public static readonly ViewRecord TestViewEnvelope2 = new("2", new TestView(2), 2L);
        
        public static ViewRecord NewTestViewEnvelopeWith(int number) => new(number.ToString(), new TestView(number), 1);

        public int Number { get; }

        public TestView(int number)
        {
            Number = number;
        }

        private bool Equals(TestView other)
        {
            return Number == other.Number;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is TestView other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Number;
        }

        public TestView Increment() => new(Number + 1);
    }
}