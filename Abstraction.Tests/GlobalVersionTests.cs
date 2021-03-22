using FluentAssertions;
using Xunit;

namespace ViewStore.Abstractions
{
    public sealed class GlobalVersionTests
    {
        [Fact]
        public void constructing_value_with_just_one_part_sets_second_part_two_zero()
        {
            (GlobalVersion.Of(12L) == GlobalVersion.Of(12L, 0L))
                .Should().BeTrue();
        }
        
        [Fact]
        public void Start_value_sets_both_parts_to_zero()
        {
            (GlobalVersion.Start == GlobalVersion.Of(0L, 0L))
                .Should().BeTrue();
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, true)]
        [InlineData(10L, 10L, 10L, 10L, true)]
        [InlineData(10L, 10L, 10L, 11L, false)]
        [InlineData(11L, 10L, 10L, 10L, false)]
        public void equal_operator_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool isEqual)
        {
            (GlobalVersion.Of(aPart1, aPart2) == GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isEqual);
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, true)]
        [InlineData(10L, 10L, 10L, 10L, true)]
        [InlineData(10L, 10L, 10L, 11L, false)]
        [InlineData(11L, 10L, 10L, 10L, false)]
        public void equals_method_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool isEqual)
        {
            GlobalVersion.Of(aPart1, aPart2).Equals(GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isEqual);
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, true)]
        [InlineData(10L, 10L, 10L, 10L, true)]
        [InlineData(10L, 10L, 10L, 11L, false)]
        [InlineData(11L, 10L, 10L, 10L, false)]
        public void equals_boxed_method_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool isEqual)
        {
            GlobalVersion.Of(aPart1, aPart2).Equals((object)GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isEqual);
        }
        
        [Fact]
        public void equals_boxed_returns_false_when_nLl_passed()
        {
            GlobalVersion.Of(1L, 1L).Equals(null!).Should().BeFalse();
        }
        
        [Fact]
        public void equals_boxed_returns_false_when_other_object_type_is_passed()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            GlobalVersion.Of(1L, 1L).Equals(new int()).Should().BeFalse();
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, false)]
        [InlineData(10L, 10L, 10L, 10L, false)]
        [InlineData(10L, 10L, 10L, 11L, true)]
        [InlineData(11L, 10L, 10L, 10L, true)]
        public void not_equal_operator_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool isNotEqual)
        {
            (GlobalVersion.Of(aPart1, aPart2) != GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(isNotEqual);
        }
        
        

        [Theory]
        [InlineData(0L, 0L, 0L, 0L, false)]
        [InlineData(10L, 10L, 10L, 10L, false)]
        [InlineData(10L, 11L, 10L, 10L, false)]
        [InlineData(11L, 10L, 10L, 10L, false)]
        [InlineData(10L, 9L, 10L, 10L, true)]
        [InlineData(9L, 10L, 10L, 11L, true)]
        public void operator_less_than_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool aLessThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) < GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aLessThanB);
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, true)]
        [InlineData(10L, 10L, 10L, 10L, true)]
        [InlineData(10L, 11L, 10L, 10L, false)]
        [InlineData(11L, 10L, 10L, 10L, false)]
        [InlineData(10L, 9L, 10L, 10L, true)]
        [InlineData(9L, 10L, 10L, 11L, true)]
        public void operator_less_or_equal_than_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool aLessOrEqualThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) <= GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aLessOrEqualThanB);
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, false)]
        [InlineData(10L, 10L, 10L, 10L, false)]
        [InlineData(10L, 10L, 10L, 11L, false)]
        [InlineData(10L, 15L, 11L, 10L, false)]
        [InlineData(10L, 11L, 10L, 10L, true)]
        [InlineData(11L, 10L, 10L, 15L, true)]
        [InlineData(11L, 9L, 10L, 15L, true)]
        [InlineData(11L, 11L, 10L, 10L, true)]
        public void operator_greater_than_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool aGreaterThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) > GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aGreaterThanB);
        }
        
        [Theory]
        [InlineData(0L, 0L, 0L, 0L, true)]
        [InlineData(10L, 10L, 10L, 10L, true)]
        [InlineData(10L, 10L, 10L, 11L, false)]
        [InlineData(10L, 15L, 11L, 10L, false)]
        [InlineData(10L, 11L, 10L, 10L, true)]
        [InlineData(11L, 10L, 10L, 15L, true)]
        [InlineData(11L, 9L, 10L, 15L, true)]
        [InlineData(11L, 11L, 10L, 10L, true)]
        public void operator_greater_or_equal_than_returns_correct_value(
            long aPart1, long aPart2,
            long bPart1, long bPart2,
            bool aGreaterOrEqualThanB)
        {
            (GlobalVersion.Of(aPart1, aPart2) >= GlobalVersion.Of(bPart1, bPart2))
                .Should().Be(aGreaterOrEqualThanB);
        }
    }
}