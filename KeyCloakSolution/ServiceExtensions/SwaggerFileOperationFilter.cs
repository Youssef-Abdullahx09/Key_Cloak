using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace KeyCloakSolution.ServiceExtensions;

public class SwaggerFileOperationFilter : IOperationFilter

{

    //

    // Parameters:

    //   operation:

    //

    //   context:

    public void Apply(OpenApiOperation operation, OperationFilterContext context)

    {

        string fileUploadMime = "multipart/form-data";

        if (operation.RequestBody != null && operation.RequestBody.Content.Any((x) => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))

        {

            IEnumerable<ParameterInfo> source = from p in context.MethodInfo.GetParameters()

                                                where p.ParameterType == typeof(IFormFile)

                                                select p;

            operation.RequestBody.Content[fileUploadMime].Schema.Properties = source.ToDictionary((k) => k.Name, (v) => new OpenApiSchema

            {

                Type = "string",

                Format = "binary"

            });

        }

    }

}

public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{

    private readonly IApiVersionDescriptionProvider _provider;

    private readonly SwaggerConfig _config;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)

    {

        _config = configuration.GetSwaggerConfig();

        _provider = provider;

    }

    public void Configure(string name, SwaggerGenOptions options)

    {

        Configure(options);

        if (_config.DocumentationEnabled)

        {

            string path = "Swagger-Documentation.xml";

            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, path));

        }

    }

    //

    // Parameters:

    //   options:

    public void Configure(SwaggerGenOptions options)

    {

        foreach (ApiVersionDescription apiVersionDescription in _provider.ApiVersionDescriptions)

        {

            options.SwaggerDoc(apiVersionDescription.GroupName, CreateVersionInfo(apiVersionDescription));

        }

    }

    private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)

    {

        OpenApiInfo openApiInfo = new OpenApiInfo

        {

            Title = _config.Title + " (" + EnvVariables.ENVIRONMENT_NAME + ")",

            Version = description.ApiVersion.ToString()

        };

        if (description.IsDeprecated)

        {

            openApiInfo.Description += " This API version has been deprecated.";

        }

        return openApiInfo;

    }

}