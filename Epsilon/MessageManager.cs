using Epsilon.Models;

namespace Epsilon;

public class MessageManager : IMessageManager
{
    public Task AddMessage(Message message)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMessage(byte[] messageId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> GetAllMessages()
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> GetMessagesBetweenUsers(byte[] userId1, byte[] userId2, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        throw new NotImplementedException();
    }

    public Task<Message> GetMessageById(byte[] messageId)
    {
        throw new NotImplementedException();
    }
}