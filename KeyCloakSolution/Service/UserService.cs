using KeyCloakSolution.Domain;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KeyCloakSolution.Service;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string keyCloakBaseUrl = "http://localhost:8080";
    private readonly string realm = "MyAppRealm";

    public UserService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetTokenFromHeaders()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            throw new Exception("Authorization header not found");
        }

        var token = authorizationHeader.Split(" ").Last();
        return token;
    }
    public async Task CreateUser(User user)
    {
        var token = GetTokenFromHeaders();
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //// Set the Authorization header with the admin token
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.PostAsync($"{keyCloakBaseUrl}/admin/realms/{realm}/users", content);
        Console.WriteLine($"Failed to create user: {response}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("User created successfully.");
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create user: {responseContent}");
        }
    }

    public async Task Delete(string userId)
    {
        var token = GetTokenFromHeaders();

        // Set the Keycloak API URL
        var url = $"{keyCloakBaseUrl}/admin/realms/{realm}/users/{userId}";

        // Add Authorization header with the Bearer token
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Send the request
        var response = await _httpClient.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("User deleted successfully.");
        }
        else
        {
            // Throwing exception with response content for debugging
            throw new Exception($"Failed to delete user: {await response.Content.ReadAsStringAsync()}");
        }
    }

    public async Task<List<User>> Get(FilterDto filter)
    {
        // Get all users from service or external API
        var token = GetTokenFromHeaders();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{keyCloakBaseUrl}/admin/realms/{realm}/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get users: {await response.Content.ReadAsStringAsync()}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<User>>(responseContent) ?? new List<User>();

        // Apply filtering by username, firstName, lastName, email, and search
        if (!string.IsNullOrEmpty(filter.username))
        {
            users = users.Where(u => u.username.Contains(filter.username, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(filter.firstName))
        {
            users = users.Where(u => u.firstName.Contains(filter.firstName, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(filter.lastName))
        {
            users = users.Where(u => u.lastName.Contains(filter.lastName, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(filter.email))
        {
            users = users.Where(u => u.email.Contains(filter.email, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(filter.search))
        {
            users = users.Where(u =>
                u.username.Contains(filter.search, StringComparison.OrdinalIgnoreCase) ||
                u.firstName.Contains(filter.search, StringComparison.OrdinalIgnoreCase) ||
                u.lastName.Contains(filter.search, StringComparison.OrdinalIgnoreCase) ||
                u.email.Contains(filter.search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Filter by enabled status
        if (filter.enabled)
        {
            users = users.Where(u => u.enabled == filter.enabled).ToList();
        }

        // Apply sorting based on the direction and field
        users = filter.direction switch
        {
            Direction.Desc when filter.sortBy == "lastname" => users.OrderByDescending(u => u.lastName).ToList(),
            Direction.Asc when filter.sortBy == "lastname" => users.OrderBy(u => u.lastName).ToList(),
            Direction.Desc when filter.sortBy == "firstname" => users.OrderByDescending(u => u.firstName).ToList(),
            Direction.Asc when filter.sortBy == "firstname" => users.OrderBy(u => u.firstName).ToList(),
            Direction.Desc when filter.sortBy == "email" => users.OrderByDescending(u => u.email).ToList(),
            Direction.Asc when filter.sortBy == "email" => users.OrderBy(u => u.email).ToList(),
            Direction.Desc => users.OrderByDescending(u => u.username).ToList(), // Default is sorting by UserName
            Direction.Asc => users.OrderBy(u => u.username).ToList(), // Default sorting by UserName in ascending order
            _ => users // If no valid sorting is provided, return unsorted list
        };

        return users;
    }


    public async Task<User?> GetById(string userId)
    {
        var token = GetTokenFromHeaders();
        // Set the Keycloak API URL
        var url = $"{keyCloakBaseUrl}/admin/realms/{realm}/users/{userId}";

        // Add Authorization header with the Bearer token
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Send the request
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            // Get the response content (user data)
            var responseData = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response into User object
            var user = JsonSerializer.Deserialize<User>(responseData);

            return user;

        }
        throw new Exception($"Failed to get users: {await response.Content.ReadAsStringAsync()}");

    }

    public async Task Update(CreateUserDto user, string id)
    {
        var updatedUser = new User
        {
            username = user.UserName,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            emailVerified = user.emailVerified,
            enabled = user.Enabled,
            credentials = new Credential[]
            {
                new Credential
                {
                    type = "password",
                    value = user.Password,
                    temporary = false
                }
            }
        };
        var token = GetTokenFromHeaders();
        var json = JsonSerializer.Serialize(updatedUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // PUT or PATCH based on Keycloak's API (typically PUT for updates)
        var response = await _httpClient.PutAsync($"{keyCloakBaseUrl}/admin/realms/{realm}/users/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update user: {responseContent}");
        }

        Console.WriteLine("User updated successfully.");
    }
}
