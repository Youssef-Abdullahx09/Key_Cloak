using KeyCloakSolution.Domain;

namespace KeyCloakSolution.Service;

public interface IUserService
{
    Task CreateUser(User user);
    Task Update(CreateUserDto user, string userId);
    Task<List<User>> Get(FilterDto filter);
    Task<User?> GetById(string id);
    //Task<User?> GetProfileById(string id);
    Task Delete(string userId);
}
