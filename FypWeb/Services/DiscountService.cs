using System;
using System.Linq;
using System.Threading.Tasks;
using Fyp.DataAccess.Data;
using FypWeb.IService;
using Microsoft.EntityFrameworkCore;

namespace FypWeb.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _db;

        public DiscountService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task UpdateExpiredDiscountsAsync()
        {
            var today = DateTime.Today;
            var expiredDiscounts = await _db.Discount
                .Where(d => d.EndDate <= today && d.IsActive)
                .ToListAsync();

            foreach (var discount in expiredDiscounts)
            {
                discount.IsActive = false;
            }

            if (expiredDiscounts.Any())
            {
                _db.Discount.UpdateRange(expiredDiscounts);
                await _db.SaveChangesAsync();
            }
        }
    }
}
