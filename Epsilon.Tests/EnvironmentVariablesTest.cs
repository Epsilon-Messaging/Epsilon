using FluentAssertions;
using Xunit;

using static Epsilon.EnvironmentVariables;

namespace Epsilon.Tests;

public class EnvironmentVariablesTest
{

    [Fact]
    public void ASPNETCORE_ENVIRONMENT_ShouldReturnEnvironment_WhenItsSet()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        ASPNETCORE_ENVIRONMENT.Should().Be("Development");
    }

    [Fact]
    public void ASPNETCORE_ENVIRONMENT_ShouldThrowAnError_WhenItsNotSet()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

        var action = () => ASPNETCORE_ENVIRONMENT;
        action.Should().Throw<ArgumentException>().WithMessage("Environment Variable ASPNETCORE_ENVIRONMENT is not set");
    }

}