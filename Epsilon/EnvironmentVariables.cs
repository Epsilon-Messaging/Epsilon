using Environment = System.Environment;

namespace Epsilon;

public static class EnvironmentVariables
{
    public static string ASPNETCORE_ENVIRONMENT => GetEnvironmentVariable(nameof(ASPNETCORE_ENVIRONMENT));

    private static string GetEnvironmentVariable(string environmentVariable)
    {
        return Environment.GetEnvironmentVariable(environmentVariable) ??
               throw new ArgumentException($"Environment Variable {environmentVariable} is not set");
    }
}