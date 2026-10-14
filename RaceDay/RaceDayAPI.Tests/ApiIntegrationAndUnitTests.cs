using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class ApiIntegrationAndUnitTests
    {
        private RaceDayDbContext GetMemoryDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var dbContext = new RaceDayDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task Register_NewUser_Returns201Created()
        {
            var context = GetMemoryDatabaseContext();
            var controller = new AuthController(context);

            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@raceday.com",
                Password = "Password123!",
                Role = "Participant"
            };

            var result = await controller.Register(registerDto);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusResult.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201Created()
        {
            var context = GetMemoryDatabaseContext();
            var organiser = new Organiser { OrganiserId = 1, UserId = 10, OrganisationName = "Cape Striders" };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            var httpContext = new DefaultHttpContext();
            var session = new TestSession();
            session.SetInt32("UserId", 10);
            session.SetString("UserRole", "Organiser");
            httpContext.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var dto = new CreateEventDto
            {
                EventName = "Table Mountain Challenge",
                EventDate = System.DateTime.UtcNow.AddDays(30),
                Location = "Cape Town"
            };

            var result = await controller.Create(dto);
            Assert.IsType<CreatedAtActionResult>(result);
        }
    }

    public class TestSession : ISession
    {
        private readonly System.Collections.Generic.Dictionary<string, byte[]> _store = new();
        public bool IsAvailable => true;
        public string Id => "test-session-id";
        public System.Collections.Generic.IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}
