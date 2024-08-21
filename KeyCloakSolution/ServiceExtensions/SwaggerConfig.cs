namespace KeyCloakSolution.ServiceExtensions;

public sealed class SwaggerConfig
{
    public bool Enabled { get; set; }

    public bool HideModels { get; set; }

    public bool DocumentationEnabled { get; set; }

    public string? Title { get; set; }
}