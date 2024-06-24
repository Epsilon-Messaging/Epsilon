using FluentAssertions;
using Xunit;

using static Epsilon.Calculator;

namespace Epsilon.Tests;

public class CalculatorTest
{
    [Theory]
    [InlineData(0,0,0)]
    [InlineData(1,5,6)]
    [InlineData(5,5,10)]
    public void Test1(int a, int b, int result)
    {
        Add(a,b).Should().Be(result);
    }
}