using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace KeyCloakSolution.ServiceExtensions;

public static class Version
{
    //
    // Parameters:
    //   services:
    public static IServiceCollection AddFortTeckApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(delegate (ApiVersioningOptions config)
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });
        services.AddVersionedApiExplorer(delegate (ApiExplorerOptions setup)
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
        return services;
    }
}
