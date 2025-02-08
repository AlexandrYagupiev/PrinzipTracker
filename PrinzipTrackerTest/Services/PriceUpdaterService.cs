using AngleSharp;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using PrinzipTrackerTest.Models;


namespace PrinzipTrackerTest.Services
{
    public class PriceUpdaterService : BackgroundService
    {
        private readonly ILogger<PriceUpdaterService> _logger;
        private readonly PrinzipContext _context;

        public PriceUpdaterService(ILogger<PriceUpdaterService> logger, PrinzipContext context)
        {
            _logger = logger;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обновление цен");

                var subscriptions = await _context.Subscriptions.ToListAsync(stoppingToken);
                foreach (var sub in subscriptions)
                {
                    var currentPrice = await GetApartmentPrice(sub.ApartmentUrl);
                    if (currentPrice.HasValue && currentPrice != sub.LastPrice)
                    {
                        SendEmailNotification(sub.Email, sub.ApartmentUrl, currentPrice.Value);
                        sub.LastPrice = currentPrice;
                        sub.LastUpdate = DateTime.Now;
                    }
                }
                await _context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task<decimal?> GetApartmentPrice(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var parser = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
                    var document = await parser.OpenAsync(req => req.Content(response));

                    var priceElement = document.QuerySelector("input[type='hidden'][name='price[min]']").Attributes["value"].Value;
                    if (priceElement != null)
                        return decimal.Parse(priceElement.Replace(" ", "").Replace("₽", ""));
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private void SendEmailNotification(string email, string apartmentUrl, decimal newPrice)
        {
            // Логика отправки email-уведомления
            _logger.LogInformation($"Отправил уведомление на {email} об изменении цен на {apartmentUrl}. Новая цена: {newPrice}");
        }
    }
}
