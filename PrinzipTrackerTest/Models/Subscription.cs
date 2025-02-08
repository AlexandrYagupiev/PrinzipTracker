using System.ComponentModel.DataAnnotations;


namespace PrinzipTrackerTest.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public string ApartmentUrl { get; set; }

        [Required]
        public string Email { get; set; }

        public decimal? LastPrice { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
