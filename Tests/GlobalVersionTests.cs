using FluentAssertions;
using ViewStore.Abstractions;
using Xunit;

namespace ViewStore.Tests
{
    public sealed class GlobalVersionTests
    {
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, true)]
        [InlineData(10UL, 10UL, 10UL, 10UL, true)]
        [InlineData(10UL, 10UL, 10UL, 11UL, false)]
        [InlineData(11UL, 10UL, 10UL, 10UL, false)]
        public void equal_operator_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool isEqual)
        {
            (GlobalVersion.Of(aPart1, aPart2) == GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isEqual);
        }
        
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, true)]
        [InlineData(10UL, 10UL, 10UL, 10UL, true)]
        [InlineData(10UL, 10UL, 10UL, 11UL, false)]
        [InlineData(11UL, 10UL, 10UL, 10UL, false)]
        public void equals_method_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool isEqual)
        {
            GlobalVersion.Of(aPart1, aPart2).Equals(GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isEqual);
        }
        
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, true)]
        [InlineData(10UL, 10UL, 10UL, 10UL, true)]
        [InlineData(10UL, 10UL, 10UL, 11UL, false)]
        [InlineData(11UL, 10UL, 10UL, 10UL, false)]
        public void equals_boxed_method_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool isEqual)
        {
            GlobalVersion.Of(aPart1, aPart2).Equals((object)GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isEqual);
        }
        
        [Fact]
        public void equals_boxed_returns_false_when_null_passed()
        {
            GlobalVersion.Of(1UL, 1UL).Equals(null!).Should().BeFalse();
        }
        
        [Fact]
        public void equals_boxed_returns_false_when_other_object_type_is_passed()
        {
            GlobalVersion.Of(1UL, 1UL).Equals(new int()).Should().BeFalse();
        }
        
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, false)]
        [InlineData(10UL, 10UL, 10UL, 10UL, false)]
        [InlineData(10UL, 10UL, 10UL, 11UL, true)]
        [InlineData(11UL, 10UL, 10UL, 10UL, true)]
        public void not_equal_operator_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool isNotEqual)
        {
            (GlobalVersion.Of(aPart1, aPart2) != GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isNotEqual);
        }
        
        

        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, false)]
        [InlineData(10UL, 10UL, 10UL, 10UL, false)]
        [InlineData(10UL, 11UL, 10UL, 10UL, false)]
        [InlineData(11UL, 10UL, 10UL, 10UL, false)]
        [InlineData(10UL, 9UL, 10UL, 10UL, true)]
        [InlineData(9UL, 10UL, 10UL, 11UL, true)]
        public void operator_less_than_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool aLessThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) < GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aLessThanB);
        }
        
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, true)]
        [InlineData(10UL, 10UL, 10UL, 10UL, true)]
        [InlineData(10UL, 11UL, 10UL, 10UL, false)]
        [InlineData(11UL, 10UL, 10UL, 10UL, false)]
        [InlineData(10UL, 9UL, 10UL, 10UL, true)]
        [InlineData(9UL, 10UL, 10UL, 11UL, true)]
        public void operator_less_or_equal_than_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool aLessOrEqualThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) <= GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aLessOrEqualThanB);
        }
        
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, false)]
        [InlineData(10UL, 10UL, 10UL, 10UL, false)]
        [InlineData(10UL, 10UL, 10UL, 11UL, false)]
        [InlineData(10UL, 15UL, 11UL, 10UL, false)]
        [InlineData(10UL, 11UL, 10UL, 10UL, true)]
        [InlineData(11UL, 10UL, 10UL, 15UL, true)]
        [InlineData(11UL, 9UL, 10UL, 15UL, true)]
        [InlineData(11UL, 11UL, 10UL, 10UL, true)]
        public void operator_greater_than_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool aGreaterThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) > GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aGreaterThanB);
        }
        
        [Theory]
        [InlineData(0UL, 0UL, 0UL, 0UL, true)]
        [InlineData(10UL, 10UL, 10UL, 10UL, true)]
        [InlineData(10UL, 10UL, 10UL, 11UL, false)]
        [InlineData(10UL, 15UL, 11UL, 10UL, false)]
        [InlineData(10UL, 11UL, 10UL, 10UL, true)]
        [InlineData(11UL, 10UL, 10UL, 15UL, true)]
        [InlineData(11UL, 9UL, 10UL, 15UL, true)]
        [InlineData(11UL, 11UL, 10UL, 10UL, true)]
        public void operator_greater_or_equal_than_returns_correct_value(
            ulong aPart1, ulong aPart2,
            ulong bPart1, ulong bPart2,
            bool aGreaterOrEqualThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) >= GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aGreaterOrEqualThanB);
        }
    }
}