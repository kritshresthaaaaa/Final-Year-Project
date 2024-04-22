using System.Threading.Tasks;
using Fyp.Models;
using System.Collections.Generic;

namespace FypWeb.Services
{
    public interface IDiscountService
    {
        Task UpdateExpiredDiscountsAsync();
    }
}
