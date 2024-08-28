using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                options.Authority = "http://localhost:8080/realms/MJ_Tech";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
                
                // options.Events = new JwtBearerEvents
                // {
                //     OnTokenValidated = context =>
                //     {
                //         var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                //
                //         // Get the JWT token
                //         var token = context.SecurityToken as JwtSecurityToken;
                //
                //         // Extract roles from realm_access
                //         var realmRoles = token?.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
                //         if (realmRoles != null)
                //         {
                //             var realmRolesObj = JObject.Parse(realmRoles);
                //             foreach (var role in realmRolesObj["roles"])
                //             {
                //                 claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                //             }
                //         }
                //
                //         // Extract roles from resource_access
                //         var resourceAccess = token?.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                //         if (resourceAccess != null)
                //         {
                //             var resourceAccessObj = JObject.Parse(resourceAccess);
                //             foreach (var client in resourceAccessObj)
                //             {
                //                 foreach (var role in client.Value["roles"])
                //                 {
                //                     claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, $"{client.Key}:{role}"));
                //                 }
                //             }
                //         }
                //
                //         return Task.CompletedTask;
                //     }
                // };
                //
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claims = new List<Claim>();
                        var user = context.Principal;
                
                        // Find the realm_access claim, which contains the roles
                        var realmAccessClaim = user?.FindFirst("realm_access")?.Value;
                
                        if (realmAccessClaim != null)
                        {
                            // Parse the realm_access claim (which is JSON) to extract the roles
                            var realmAccess = JsonConvert.DeserializeObject<RealmAccess>(realmAccessClaim);
                
                            // Convert each role into a Claim of type ClaimTypes.Role
                            claims = realmAccess.Roles?.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
                        }
                        
                
                        // Find the realm_access claim, which contains the roles
                        var resourceAccessClaim = user?.FindFirst("resource_access")?.Value;
                
                        if (resourceAccessClaim != null)
                        {
                            // Parse the realm_access claim (which is JSON) to extract the roles
                            var resourceAccess = JsonConvert.DeserializeObject<Dictionary<string, RoleAccess>>(resourceAccessClaim);
                
                            // Convert each role into a Claim of type ClaimTypes.Role
                            var roleClaims = resourceAccess?.Values
                                .SelectMany(x => x.Roles)
                                .Select(role =>  new Claim(ClaimTypes.Role, role));

                            if (roleClaims is not null)
                            {
                                claims.AddRange(roleClaims);
                                
                            }
                        }
                        

                        var appIdentity = new ClaimsIdentity(claims);
                        user?.AddIdentity(appIdentity);
                
                        return Task.CompletedTask;
                    }
                };
            });

    }
    public class ResourceAccess
    {
        [JsonProperty("resource_access")]
        public Dictionary<string, RoleAccess> Access { get; set; }
    }

    public class RoleAccess
    {
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }
    public static IServiceCollection AddFortTeckSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetSwaggerConfig().Enabled)
        {
            return services;
        }

        services.AddSwaggerGen(delegate(SwaggerGenOptions option)
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
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
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
                }
            });
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
        app.UseSwaggerUI(delegate(SwaggerUIOptions c)
        {
            int depth = ((!config.HideModels) ? 1 : (-1));
            c.DocExpansion(DocExpansion.None);
            c.DefaultModelsExpandDepth(depth);
        });
        return app;
    }
}