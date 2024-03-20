using Fyp.Models;

namespace FypWeb.IService
{
    public interface INotiService
    {
        List<Noti> GetNotifications(Guid nToEmployeeId, bool bIsGetOnlyUnread);
     
    }
}
