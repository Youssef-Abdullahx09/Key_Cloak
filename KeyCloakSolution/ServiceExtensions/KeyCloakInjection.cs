using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Cryptography;

namespace KeyCloakSolution.ServiceExtensions;

public static class KeyCloakExtension
{
    public static void AddKeycloak(this WebApplicationBuilder builder)

    {
        IdentityModelEventSource.ShowPII = true;

        var base64PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvei7jcuANpDZz/jbAE9DCDqW7HmE6fM5XCLfTaJ9frBfSqMRqQhxBQm8KWnD0/0hj4ZaYS3McM0FRdo9DdP/MMJCLagxVaCam9Gwe43wjCqmGRGmNoVOhdSNVsfUgMDxOdopegWe2dxfYxThBcVzOB9fdMg5ULLq8VWeAkF9gqEibEnu6Fv0nFTbKCq3BG/PSN+nuDiWFWHk4bluNJkSfwFO85DpiLU9SJkPFksSVXlfW8u0lucl0pRDu+rIIseSspOzbCYDeFUSJ6wzNllWRtICVaclP/VK3Ahd/fmdnnX1Mr6hW0uFAYLR7QNCukyeTQJ/4oKJ1TNOR33CdmhjzwIDAQAB";

        var rsa = CreateRsaProviderFromPublicKey(base64PublicKey);

        builder.Services
            .AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = "http://localhost:8080/auth/realms/Cloak_Realm";
                options.SaveToken = false;
                options.RequireHttpsMetadata = false;
                options.Audience = "Cloak_Client";


                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost:8080/realms/Cloak_Realm",
                    ValidAudience = "Cloak_Client",
                    IssuerSigningKey = new SymmetricSecurityKey()
                };
            });
    }

    public static RSA CreateRsaProviderFromPublicKey(string base64PublicKey)
    {
        var rsa = RSA.Create();
        var publicKeyBytes = Convert.FromBase64String(base64PublicKey);
        rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        return rsa;
    }


    public static IServiceCollection AddFortTeckSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetSwaggerConfig().Enabled)
        {
            return services;
        }

        services.AddSwaggerGen(delegate (SwaggerGenOptions option)
        {
            option.CustomSchemaIds((Type x) => x.FullName);
            option.OperationFilter<SwaggerFileOperationFilter>(Array.Empty<object>());
            option.MapType<DateTime>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date"
            });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement {
         {
             new OpenApiSecurityScheme
             {
                 Reference = new OpenApiReference
                 {
                     Type = ReferenceType.SecurityScheme,
                     Id = "Bearer"
                 }
             },
             new string[0]
         } });
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return services;
    }

    //
    // Parameters:
    //   app:
    //
    //   configuration:
    //
    // Exceptions:
    //   T:System.Exception:
    public static WebApplication UseFortTeckSwagger(this WebApplication app, IConfiguration configuration)
    {
        SwaggerConfig config = configuration.GetSwaggerConfig();
        if (!config.Enabled)
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(delegate (SwaggerUIOptions c)
        {
            int depth = ((!config.HideModels) ? 1 : (-1));
            c.DocExpansion(DocExpansion.None);
            c.DefaultModelsExpandDepth(depth);
        });
        return app;
    }
}