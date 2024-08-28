using KeyCloakSolution.Domain;
using KeyCloakSolution.Service;
using KeyCloakSolution.ServiceExtensions;
using KeyCloakSolution.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<KeyCloakSetting>(builder.Configuration.GetSection(nameof(KeyCloakSetting)));
builder.AddKeycloak();
builder.Services.AddFortTeckApiVersioning();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFortTeckSwagger(builder.Configuration);


builder.Services.AddHttpClient();
builder.Services.AddScoped<IKeycloakTokenService, KeycloakTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
