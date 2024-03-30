using Fyp.DataAccess.Data;
using Fyp.Models;
using FypWeb.IService;
using Microsoft.EntityFrameworkCore; // Ensure you have this for EF Core
using System;
using System.Collections.Generic;
using System.Linq;

namespace FypWeb.Services
{
    public class NotiService : INotiService
    {
        private readonly ApplicationDbContext _dbContext; // Use dependency injection for DbContext

        // Constructor that accepts DBContext for dependency injection
        public NotiService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Noti>> GetNotifications(Guid nToEmployeeId, bool bIsGetOnlyUnread)
        {
            List<Noti> _oNotifications = new List<Noti>(); // No need to initialize it as a field

            if (bIsGetOnlyUnread)
            {
                _oNotifications = await _dbContext.Notification
                    .Where(x => x.ToEmployeeId == nToEmployeeId && !x.IsRead)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
            }
            else
            {
                _oNotifications = await _dbContext.Notification
                    .Where(x => x.ToEmployeeId == nToEmployeeId)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
            }

            return _oNotifications; // Corrected to return the list of notifications
        }
    }
}
