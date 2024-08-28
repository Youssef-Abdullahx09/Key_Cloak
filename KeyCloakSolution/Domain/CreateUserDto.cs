namespace KeyCloakSolution.Domain;

public class CreateUserDto
{
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool Enabled { get; set; } = default!;
    public bool emailVerified { get; set; } = default!;
    public string Password { get; set; } = default!;
}
