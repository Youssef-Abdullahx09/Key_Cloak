using KeyCloakSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KeyCloakSolution.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IKeycloakTokenService keycloakTokenService;


    public UserController(IKeycloakTokenService keycloakTokenService)
    {
        this.keycloakTokenService = keycloakTokenService;
    }


    [HttpPost("token")]
    public async Task<IActionResult> AuthorizeAsync([FromBody] KeycloakUserDto keycloakUserDto)
    {
        try
        {
            var response = await keycloakTokenService
                .GetTokenResponseAsync(keycloakUserDto)
                .ConfigureAwait(false);

            return new OkObjectResult(response);
        }
        catch (Exception)
        {
            return BadRequest("An error has occured!");
        }
    }

    [Authorize]
    [HttpGet("check/authorization")]
    public IActionResult CheckKeycloakAuthorization()
    {
        return new OkObjectResult(HttpStatusCode.OK);
    }

    [HttpGet("signin-oidc")]
    public IActionResult SigninOidc()
    {
        return new OkObjectResult(HttpStatusCode.OK);
    }

    [Authorize(Roles = "offline_access")]
    [HttpGet]
    public IActionResult AdminOnly()
    {
        return Ok("Admin access granted.");
    }

    [HttpGet("GetSessionId")]
    public async Task<IActionResult> GetSessionId(string accessToken)
    {
        // Replace these variables with your actual values
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";
        string clientId = "app-client";
        string clientSecret = "XLMxL9Boc1NPTEnuxVJEsbHVTLnj0Zop";

        var tokenInfo = new TokenIntrospectionResponse();
        // Create the HttpClient
        using (HttpClient client = new HttpClient())
        {
            // Set the URL for the introspection endpoint
            string introspectUrl = $"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/token/introspect";

            // Prepare the request content
            var requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", accessToken),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            // Send the POST request
            HttpResponseMessage response = await client.PostAsync(introspectUrl, requestData);

            // Check the response status code
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                var responseContent = await response.Content.ReadAsStringAsync();

                var info = JObject.Parse(responseContent);

                // Parse the response content as JSON (optional)
                tokenInfo = JsonConvert.DeserializeObject<TokenIntrospectionResponse>(responseContent);

                // Output the parsed JSON
                Console.WriteLine("Token Info: " + tokenInfo);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        return Ok(tokenInfo);
    }


    [HttpPost("revoke-session")]
    public async Task<IActionResult> RevokeSession(string token)
    {
        // Replace with your actual Keycloak URL and Realm
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";
        string clientId = "app-client";
        string clientSecret = "XLMxL9Boc1NPTEnuxVJEsbHVTLnj0Zop";
        string endSessionUrl = $"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/logout";

        // Prepare the request content
        var requestData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("refresh_token", token)
        });

        using (var _httpClient = new HttpClient())
        {
            // Send the POST request to revoke the session
            HttpResponseMessage response = await _httpClient.PostAsync(endSessionUrl, requestData);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Session revoked successfully." });
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode,
                    new { message = "Failed to revoke session.", details = responseContent });
            }
        }
    }


    [HttpGet("get-accessToken-From-refreshToken")]
    public async Task<IActionResult> GetNewAccessTokenUsingRefreshTokenAsync(string refreshToken)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";
        string clientId = "app-client";
        string clientSecret = "XLMxL9Boc1NPTEnuxVJEsbHVTLnj0Zop";
        string url = $"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/token";

        // Prepare the request content
        var requestData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });

        using (var _httpClient = new HttpClient())
        {
            // Send the POST request
            HttpResponseMessage response = await _httpClient.PostAsync(url, requestData);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read and parse the response content
                string responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                return Ok(tokenResponse); // Return as a JObject for further use
            }
            else
            {
                // Handle the error as needed
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error refreshing token: {errorContent}");
            }
        }
    }


    [HttpGet("admin-logging-out-user-from-keycloak-admin-panel")]
    public async Task<IActionResult> LoggingOutUserFromAdminPanel(string userId, string adminAccessToken)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";
        // string clientId = "app-client";
        // string clientSecret = "XLMxL9Boc1NPTEnuxVJEsbHVTLnj0Zop";
        string url = $"{keycloakUrl}/admin/realms/{realmName}/users/{userId}/logout";

        using (var _httpClient = new HttpClient())
        {
            // Set the Authorization header with the admin access token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

            // Send the POST request to log out the user
            HttpResponseMessage response = await _httpClient.PostAsync(url, null);

            return Ok(response.IsSuccessStatusCode);
        }
    }


    [HttpGet("revoke-token-by-admin")]
    // public async Task<IActionResult> RevokeTokenAsync(string token, string adminAccessToken)
    // {
    //     string keycloakUrl = "http://localhost:8080";
    //     string realmName = "MJ_Tech";
    //     string url = $"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/revoke";
    //
    //     using (var _httpClient = new HttpClient())
    //     {
    //         var request = new HttpRequestMessage(HttpMethod.Post, url)
    //         {
    //             Content = new FormUrlEncodedContent(new[]
    //             {
    //                 new KeyValuePair<string, string>("token", token),
    //                 new KeyValuePair<string, string>("token_type_hint", "access_token")
    //             })
    //         };
    //
    //         // Set the Authorization header with the admin access token
    //         _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
    //
    //         HttpResponseMessage response = await _httpClient.SendAsync(request);
    //
    //         if (response.IsSuccessStatusCode)
    //         {
    //             return Ok("Token revoked successfully");
    //         }
    //         else
    //         {
    //             return StatusCode((int)response.StatusCode, "Failed to revoke token");
    //         }
    //     }
    // }
    public async Task RevokeTokenAsync(string token)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{keycloakUrl}/realms/{realmName}/protocol/openid-connect/revoke");

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{"app-client"}:{"XLMxL9Boc1NPTEnuxVJEsbHVTLnj0Zop"}")));

        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", token)
        });

        using (var _httpClient = new HttpClient())
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                // Handle token revocation failure
                throw new Exception("Failed to revoke token: " + response.ReasonPhrase);
            }
        }
    }


    [HttpGet("GET-USER-ROLES")]
    public async Task<IActionResult> GetUserRolesAsync(string userId, string adminAccessToken)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";

        var rolesEndpoint = $"{keycloakUrl}/admin/realms/{realmName}/users/{userId}/role-mappings/clients/account";

        var request = new HttpRequestMessage(HttpMethod.Get, rolesEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        using (var _httpClient = new HttpClient())
        {
            var response = await _httpClient.SendAsync(request);
            Console.WriteLine(request);
            Console.WriteLine(response);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<IEnumerable<string>>(responseContent);

            return Ok(roles);
        }
    }

    [HttpGet("Get-Realm-Clients")]
    public async Task<IActionResult> GetRealmClients(string adminAccessToken)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";

        var rolesEndpoint = $"{keycloakUrl}/admin/realms/{realmName}/clients/";

        var request = new HttpRequestMessage(HttpMethod.Get, rolesEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        using (var _httpClient = new HttpClient())
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var clients = JsonConvert.DeserializeObject<IEnumerable<RoleRepresentation>>(responseContent);

            return Ok(clients);
        }
    }

    [HttpGet("Get-Client-Roles")]
    public async Task<IActionResult> GetClientRoles(string clientUUId, string adminAccessToken)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";

        var rolesEndpoint = $"{keycloakUrl}/admin/realms/{realmName}/clients/{clientUUId}/roles";

        var request = new HttpRequestMessage(HttpMethod.Get, rolesEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        using (var _httpClient = new HttpClient())
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var clients = JsonConvert.DeserializeObject<IEnumerable<RoleRepresentation>>(responseContent);

            return Ok(clients);
        }
    }

    [HttpGet("Get-User-Roles")]
    public async Task<IActionResult> GetUserRoles(string userId, string adminAccessToken)
    {
        string keycloakUrl = "http://localhost:8080";
        string realmName = "MJ_Tech";

        var rolesEndpoint = $"{keycloakUrl}/admin/realms/{realmName}/users/{userId}/role-mappings";

        var request = new HttpRequestMessage(HttpMethod.Get, rolesEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        using (var _httpClient = new HttpClient())
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var clients = JsonConvert.DeserializeObject<IEnumerable<RoleRepresentation>>(responseContent);

            return Ok(clients);
        }
    }

    [HttpPost("Add-Realm-Level-Role")]
    public async Task<IActionResult> AssignRealmRoleAsync(string userId, string adminAccessToken, AddRoleDto role)
    {
        string _baseUrl = "http://localhost:8080";
        string _realm = "MJ_Tech";

        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";

        var requestContent =
            new StringContent(JsonConvert.SerializeObject(new[] { role }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminAccessToken);
        request.Content = requestContent;

        using var _httpClient = new HttpClient();
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return Ok(response);
    }

    [HttpDelete("Revoke-Realm-Level-Role")]
    public async Task<IActionResult> RevokeRealmRoleAsync(string userId, AddRoleDto role, string adminAccessToken)
    {
        string _baseUrl = "http://localhost:8080";
        string _realm = "MJ_Tech";

        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";

        var requestContent =
            new StringContent(JsonConvert.SerializeObject(new[] { role }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminAccessToken);
        request.Content = requestContent;

        using var _httpClient = new HttpClient();
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return Ok(response.StatusCode);
    }

    [HttpPost("Add-Client-Level-Role")]
    public async Task<IActionResult> AssignClientRoleAsync(string userId, string clientId, AddRoleDto role,
        string adminAccessToken)
    {
        string _baseUrl = "http://localhost:8080";
        string _realm = "MJ_Tech";

        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/clients/{clientId}";

        var requestContent =
            new StringContent(JsonConvert.SerializeObject(new[] { role }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminAccessToken);
        request.Content = requestContent;

        using var _httpClient = new HttpClient();
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return Ok(response.StatusCode);
    }

    [HttpDelete("Revoke-Client-Level-Role")]
    public async Task<IActionResult> RevokeClientRoleAsync(string userId, string clientId, AddRoleDto role,
        string adminAccessToken)
    {
        string _baseUrl = "http://localhost:8080";
        string _realm = "MJ_Tech";

        var url = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/clients/{clientId}";

        var requestContent =
            new StringContent(JsonConvert.SerializeObject(new[] { role }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminAccessToken);
        request.Content = requestContent;

        using var _httpClient = new HttpClient();
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return Ok(response.StatusCode);
    }


    public class AddRoleDto
    {
        [JsonProperty("id")] public string? Id { get; set; }

        [JsonProperty("name")] public string? Name { get; set; }
    }

    public class RoleRepresentation
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("composite")] public bool Composite { get; set; }

        [JsonProperty("clientRole")] public bool ClientRole { get; set; }

        [JsonProperty("containerId")] public string ContainerId { get; set; }
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; }

        [JsonProperty("expires_in")] public int ExpiresIn { get; set; }

        [JsonProperty("refresh_expires_in")] public int RefreshExpiresIn { get; set; }

        [JsonProperty("refresh_token")] public string RefreshToken { get; set; }

        [JsonProperty("token_type")] public string TokenType { get; set; }

        [JsonProperty("not-before-policy")] public int NotBeforePolicy { get; set; }

        [JsonProperty("session_state")] public string SessionState { get; set; }

        [JsonProperty("scope")] public string Scope { get; set; }
    }

    public class TokenIntrospectionResponse
    {
        [JsonProperty("exp")] public long Exp { get; set; }

        [JsonProperty("iat")] public long Iat { get; set; }

        [JsonProperty("jti")] public string Jti { get; set; }

        [JsonProperty("iss")] public string Iss { get; set; }

        [JsonProperty("aud")] public string Aud { get; set; }

        [JsonProperty("sub")] public string Sub { get; set; }

        [JsonProperty("typ")] public string Typ { get; set; }

        [JsonProperty("azp")] public string Azp { get; set; }

        [JsonProperty("sid")] public string Sid { get; set; }

        [JsonProperty("acr")] public string Acr { get; set; }

        [JsonProperty("allowed-origins")] public List<string> AllowedOrigins { get; set; }

        [JsonProperty("realm_access")] public RealmAccess RealmAccess { get; set; }

        [JsonProperty("resource_access")] public ResourceAccess ResourceAccess { get; set; }

        [JsonProperty("scope")] public string Scope { get; set; }

        [JsonProperty("email_verified")] public bool EmailVerified { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("preferred_username")] public string PreferredUsername { get; set; }

        [JsonProperty("given_name")] public string GivenName { get; set; }

        [JsonProperty("family_name")] public string FamilyName { get; set; }

        [JsonProperty("email")] public string Email { get; set; }

        [JsonProperty("client_id")] public string ClientId { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("token_type")] public string TokenType { get; set; }

        [JsonProperty("active")] public bool Active { get; set; }
    }

    public class RealmAccess
    {
        [JsonProperty("roles")] public List<string> Roles { get; set; }
    }

    public class ResourceAccess
    {
        [JsonProperty("account")] public Account Account { get; set; }
    }

    public class Account
    {
        [JsonProperty("roles")] public List<string> Roles { get; set; }
    }
}