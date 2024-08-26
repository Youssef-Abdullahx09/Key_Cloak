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

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        })
  .AddJwtBearer(options =>
  {
      options.Authority = "http://localhost:8080/realms/MyAppRealm";
      options.Audience = "account"; 
      options.RequireHttpsMetadata = false;
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidIssuer = "http://localhost:8080/realms/MyAppRealm",
          ValidateAudience = true,
          ValidAudience = "account",
          ValidateLifetime = true,
          RoleClaimType = "roles"
      };
  });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("User", policy => policy.RequireRole("user"));
        });
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