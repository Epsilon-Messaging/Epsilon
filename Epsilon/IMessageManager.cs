using Epsilon.Models;

namespace Epsilon;

public interface IMessageManager
{
    public Task AddMessage(Message message);
    public Task DeleteMessage(byte[] messageId);
    public Task<List<Message>> GetAllMessages();
    public Task<List<Message>> GetMessagesBetweenUsers(byte[] userId1, byte[] userId2, DateTime? startDate = null, DateTime? endDate = null);
    public Task<Message> GetMessageById(byte[] messageId);
}