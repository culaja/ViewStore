namespace ViewStore.Abstractions
{
    public sealed class TestView : IView
    {
        public static readonly ViewEnvelope TestViewEnvelope1 = new("1", new TestView(1), GlobalVersion.Of(1), MetaData.New());
        public static readonly ViewEnvelope TestViewEnvelope2 = new("2", new TestView(2), GlobalVersion.Of(2), MetaData.New());
        
        public static ViewEnvelope NewTestViewEnvelopeWith(int number) => new(number.ToString(), new TestView(number), GlobalVersion.Of(1), MetaData.New());

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