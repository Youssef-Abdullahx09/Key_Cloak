using KeyCloakSolution.Options;
using KeyCloakSolution.ServiceExtensions;
using KeyCloakSolution.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<KeycloakSetting>(builder.Configuration.GetSection(nameof(KeycloakSetting)));
builder.AddKeycloak();
builder.Services.AddFortTeckApiVersioning();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFortTeckSwagger(builder.Configuration);


builder.Services.AddHttpClient();
builder.Services.AddScoped<IKeycloakTokenService, KeycloakTokenService>();

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
