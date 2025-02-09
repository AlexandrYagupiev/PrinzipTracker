using AngleSharp;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using PrinzipTrackerTest.Models;
using MimeKit;
using MailKit.Net.Smtp;


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
                        await SendEmailNotification(sub.Email, sub.ApartmentUrl, currentPrice.Value);
                        sub.LastPrice = currentPrice;
                        sub.LastUpdate = DateTime.Now;
                    }
                }
                await _context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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

        private async Task SendEmailNotification(string email, string apartmentUrl, decimal newPrice)
        {
            // Логика отправки email-уведомления
            using var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("ТестоваяРассылка", "ПочтаОтправителя"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $"Отправил уведомление на {email} об изменении цен на {apartmentUrl}. Новая цена: {newPrice}"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.mail.ru", 465, true);
                await client.AuthenticateAsync("ЛогинОтПочтыОтправителя", "ПарольОтПочтыОтправителя");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }

            _logger.LogInformation($"Отправил уведомление на {email} об изменении цен на {apartmentUrl}. Новая цена: {newPrice}");
        }
    }
}
