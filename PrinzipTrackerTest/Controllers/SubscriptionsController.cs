using Microsoft.AspNetCore.Mvc;
using PrinzipTrackerTest.Models;
using Microsoft.EntityFrameworkCore;
using AngleSharp;


namespace PrinzipTrackerTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly PrinzipDbContext _context;
        private readonly HttpClient _httpClient;

        public SubscriptionsController(PrinzipDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        // POST api/subscriptions
        [HttpPost]
        public async Task<ActionResult<Subscription>> SubscribeAsync(Subscription subscription)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingSubscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.ApartmentUrl == subscription.ApartmentUrl && s.Email == subscription.Email);
            if (existingSubscription != null)
                return Conflict();

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubscriptionAsync), new { id = subscription.Id }, subscription);
        }

        // GET api/subscriptions/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Subscription>> GetSubscriptionAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return NotFound();

            return subscription;
        }

        // GET api/subscriptions/currentprices
        [HttpGet("currentprices")]
        public async Task<IActionResult> GetCurrentPricesAsync()
        {
            var subscriptions = await _context.Subscriptions.ToListAsync();
            foreach (var sub in subscriptions)
            {
                var price = await GetApartmentPriceAsync(sub.ApartmentUrl);
                if (price.HasValue)
                    sub.LastPrice = price.Value;
                sub.LastUpdate = DateTime.Now;
            }
            await _context.SaveChangesAsync();

            return Ok(subscriptions.Select(s => new { s.ApartmentUrl, s.LastPrice }));
        }

        private async Task<decimal?> GetApartmentPriceAsync(string url)
        {
            try
            {
                    var response = await _httpClient.GetStringAsync(url);
                    var parser = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
                    var document = await parser.OpenAsync(req => req.Content(response));

                    var priceElement = document.QuerySelector(".price");
                    if (priceElement != null)
                        return decimal.Parse(priceElement.TextContent.Replace(" ", "").Replace("₽", ""));
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
