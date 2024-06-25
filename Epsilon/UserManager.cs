using Epsilon.Models;

namespace Epsilon;

public class UserManager : IUserManager
{
    public async Task AddUser(User user)
    {
        await Task.Delay(0);
    }

    public async Task DeleteUser(byte[] userId)
    {
        await Task.Delay(0);
    }

    public async Task<List<User>> GetAllUsers()
    {
        await Task.Delay(0);
        return new List<User>();
    }

    public async Task<User> GetUserById(byte[] userId)
    {  
        await Task.Delay(0);
        return new User();
    }
}