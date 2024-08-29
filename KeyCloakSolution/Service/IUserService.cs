using KeyCloakSolution.Domain;

namespace KeyCloakSolution.Service;

public interface IUserService
{
    Task CreateUser(User user);
    Task Update(CreateUserDto user, string userId);
    Task<List<User>> Get(FilterDto filter);
    Task<User?> GetById(string id);
    Task Delete(string userId);
    Task<int> Count();
    Task<bool> ResetPassword(string newPassword, string userId);
    Task<bool> SendPasswordResetEmailAsync(string userId);
    Task<bool> VerifyEmail(string userId);
}
