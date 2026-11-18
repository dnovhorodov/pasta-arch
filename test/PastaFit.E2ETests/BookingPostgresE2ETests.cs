using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using PastaFit.Core.Domain;
using Testcontainers.PostgreSql;

namespace PastaFit.E2ETests;

public sealed class BookingPostgresE2ETests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString()
                    });
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Full_E2E_Lifecycle_With_Real_PostgreSQL_Adapter()
    {
        // 1. Query seeded members & classes from real PostgreSQL
        var members = await _client.GetFromJsonAsync<List<Member>>("/members");
        var classes = await _client.GetFromJsonAsync<List<ClassAvailability>>("/classes");

        Assert.NotNull(members);
        Assert.NotNull(classes);

        var alice = members!.First(m => m.IsActive);
        var bobInactive = members!.First(m => !m.IsActive);
        var spinClass = classes!.First(c => c.Name == "Spin");

        // 2. Inactive member fails with 422 UnprocessableEntity
        var inactiveResp = await _client.PostAsJsonAsync("/bookings", new
        {
            memberId = bobInactive.Id.Value,
            classId = spinClass.Id
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, inactiveResp.StatusCode);

        // 3. Active member books Spin class -> 201 Created & persisted in PostgreSQL
        var bookResp = await _client.PostAsJsonAsync("/bookings", new
        {
            memberId = alice.Id.Value,
            classId = spinClass.Id
        });
        Assert.Equal(HttpStatusCode.Created, bookResp.StatusCode);
        var booking = await bookResp.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(booking);

        // 4. Available spots in PostgreSQL decremented by 1
        var updatedClasses = await _client.GetFromJsonAsync<List<ClassAvailability>>("/classes");
        var updatedSpin = updatedClasses!.First(c => c.Id == spinClass.Id);
        Assert.Equal(spinClass.Available - 1, updatedSpin.Available);

        // 5. Cancel booking in PostgreSQL -> 204 NoContent
        var cancelResp = await _client.DeleteAsync($"/bookings/{booking!.Id.Value}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);
    }
}
