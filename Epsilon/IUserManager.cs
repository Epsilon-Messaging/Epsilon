using Epsilon.Models;

namespace Epsilon;

public interface IUserManager
{ 
    Task AddUser(User user);
    Task DeleteUser(byte[] userId);
    Task<List<User>> GetAllUsers();
    Task<User> GetUserById(byte[] userId);
}