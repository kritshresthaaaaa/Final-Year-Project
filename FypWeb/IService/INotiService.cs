using Fyp.Models;

namespace FypWeb.IService
{
    public interface INotiService
    {
        Task<List<Noti>> GetNotifications(Guid nToEmployeeId, bool bIsGetOnlyUnread);
    }
}