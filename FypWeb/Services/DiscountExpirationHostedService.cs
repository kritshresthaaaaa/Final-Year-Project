using FypWeb.Areas.Admin.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace FypWeb.Services
{
    public class DiscountExpirationHostedService : BackgroundService
    {
        private readonly ILogger<DiscountExpirationHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DiscountExpirationHostedService(ILogger<DiscountExpirationHostedService> logger ,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DiscountExpirationHostedService is starting.");  

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DiscountExpirationHostedService is stopping.");

            return Task.CompletedTask;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DiscountExpirationHostedService is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Get the IDiscountService from the service provider
                using (var scope = _serviceProvider.CreateScope())
                {
                    var discountService = scope.ServiceProvider.GetRequiredService<IDiscountService>();

                    // Call the UpdateExpiredDiscounts method
                    await discountService.UpdateExpiredDiscountsAsync();
                }

                // Delay for 24 hours before running again
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("DiscountExpirationHostedService is stopping.");
        }

    }
}
