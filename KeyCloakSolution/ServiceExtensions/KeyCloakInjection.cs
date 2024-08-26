using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace KeyCloakSolution.ServiceExtensions;

public static class KeyCloakExtension
{
    public static void AddKeycloak(this WebApplicationBuilder builder)

    {
        IdentityModelEventSource.ShowPII = true;
        
        // builder.Services
        //     .AddAuthentication(option =>
        //     {
        //         option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //         option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //         option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //     })
        //     .AddJwtBearer(options =>
        //     {
        //         options.MetadataAddress = "http://localhost:8080/realms/Default_Realm/.well-known/openid-configuration";
        //         options.Authority = "http://localhost:8080/realms/Default_Realm/protocol/openid-connect/auth";
        //
        //         options.RequireHttpsMetadata = false;
        //         
        //         options.Audience = "account";
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuerSigningKey = true,
        //             ValidateIssuer = false,
        //             ValidateAudience = false,
        //             ValidateLifetime = true,
        //             RequireExpirationTime = true,
        //             ClockSkew = TimeSpan.Zero   
        //         };
        //     });
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = "http://localhost:8080/realms/Default_Realm";
            options.Audience = "account"; 
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "http://localhost:8080/realms/Default_Realm",
                ValidateAudience = true,
                ValidAudience = "account",
                ValidateLifetime = true
            };
        });
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
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