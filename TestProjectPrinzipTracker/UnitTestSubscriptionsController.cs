using Xunit;
using Moq;
using PrinzipTrackerTest.Models;
using Microsoft.AspNetCore.Mvc;
using PrinzipTrackerTest.Controllers;
using Microsoft.EntityFrameworkCore;


namespace TestProjectPrinzipTracker
{
    public class UnitTestSubscriptionsController
    {
        private readonly PrinzipContext _context;
        private readonly SubscriptionsController _controller;

        public UnitTestSubscriptionsController()
        {

        }

        [Fact]
        public async Task Subscribe_ShouldSaveSubscription()
        {
            var subscription = new Subscription
            {
                Id = 1,
                ApartmentUrl = "test-apartment-url",
                Email = "test@gmail.com"
            };
            var result = await _controller.Subscribe(subscription);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(subscription.ApartmentUrl, result.Value.ApartmentUrl);
            Assert.Equal(subscription.Email, result.Value.Email);
        }
    }
}