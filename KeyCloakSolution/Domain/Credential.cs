namespace KeyCloakSolution.Domain;

public class Credential
{
    public string type { get; set; } = default!;
    public string value { get; set; } = default!;
    public bool temporary { get; set; }
}