using Epsilon.Models;

namespace Epsilon;

public class MessageManager : IMessageManager
{
    public async Task AddMessage(Message message)
    {
        await Task.Delay(0);
    }

    public async Task DeleteMessage(byte[] messageId)
    {
        await Task.Delay(0);
    }

    public async Task<List<Message>> GetAllMessages()
    {
        await Task.Delay(0);
        return new List<Message>();
    }

    public async Task<List<Message>> GetMessagesBetweenUsers(byte[] userId1, byte[] userId2, DateTime? startDate = null,
        DateTime? endDate = null)
    {
        await Task.Delay(0);
        return new List<Message>();
    }

    public async Task<Message> GetMessageById(byte[] messageId)
    {
        await Task.Delay(0);
        return new Message();
    }

}