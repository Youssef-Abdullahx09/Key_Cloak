namespace KeyCloakSolution.ServiceExtensions;
public static class EnvVariables
{
    public static string? ENVIRONMENT_NAME = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
}