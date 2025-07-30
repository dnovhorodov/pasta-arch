using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PastaFit.Core.Domain;

namespace PastaFit.ComponentTests;

public class BookingComponentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BookingComponentTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Can_Get_Classes()
    {
        var response = await _client.GetAsync("/classes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Yoga", json);
    }

    [Fact]
    public async Task Can_Book_And_Cancel_Class()
    {
        var members = await _client.GetFromJsonAsync<List<Member>>("/members");
        var member = members!.First(m => m.IsActive);

        var classes = await _client.GetFromJsonAsync<List<ClassAvailability>>("/classes");
        var cls = classes!.First();

        var bookingReq = new { memberId = member.Id, classId = cls.Id };

        var bookResp = await _client.PostAsJsonAsync("/bookings", bookingReq);
        Assert.Equal(HttpStatusCode.Created, bookResp.StatusCode);

        var booking = await bookResp.Content.ReadFromJsonAsync<Booking>();

        var cancelResp = await _client.DeleteAsync($"/bookings/{booking!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);
    }
}

public record ClassAvailability(Guid Id, string Name, int Capacity, int Available);
