using Newtonsoft.Json;

namespace KeyCloakSolution.ServiceExtensions;

public class RealmAccess
{
    [JsonProperty("roles")]
    public List<string> Roles { get; set; }
}