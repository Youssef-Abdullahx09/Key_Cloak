namespace KeyCloakSolution.ServiceExtensions;
public static class ConfigurationExtension
{
    //
    // Parameters:
    //   configuration:
    public static SwaggerConfig GetSwaggerConfig(this IConfiguration configuration)
    {
        return configuration.GetSection("Swagger").Get<SwaggerConfig>() ??
            throw new Exception("Missing 'Swagger' configuration section from the appsettings.");
    }
}