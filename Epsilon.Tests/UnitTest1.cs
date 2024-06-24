using FluentAssertions;
using Xunit;

using static Epsilon.Calculator;

namespace Epsilon.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Add(5,5).Should().Be(10);
    }
}