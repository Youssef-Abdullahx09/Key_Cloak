using System.Net;

namespace KeyCloakSolution.Domain;

public class User
{
    public string id { get; set; }
    public string username { get; set; } = default!;
    public string firstName { get; set; } = default!;
    public string lastName { get; set; } = default!;
    public string email { get; set; } = default!;
    public bool emailVerified  { get; set; } = default!;
    public bool enabled { get; set; } = default!;
    public Credential[] credentials { get; set; } = default!;
}
