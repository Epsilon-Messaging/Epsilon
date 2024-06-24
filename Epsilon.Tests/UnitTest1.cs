using FluentAssertions;
using Xunit;

namespace Epsilon.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        const int result = 5 + 5;

        result.Should().Be(10);
    }
}