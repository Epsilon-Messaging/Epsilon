using Epsilon.Models;

namespace Epsilon;

public class UserManager : IUserManager
{
    public Task AddUser(User user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUser(byte[] userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetAllUsers()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserById(byte[] userId)
    {  
        throw new NotImplementedException();
    }
}