using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PastaFit.Core.Domain;

namespace PastaFit.ComponentTests;

public class BookingComponentTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Can_Get_Classes()
    {
        var response = await _client.GetAsync("/classes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var classes = await response.Content.ReadFromJsonAsync<List<ClassAvailability>>();
        Assert.NotNull(classes);
        Assert.Contains(classes!, c => c.Name == "Yoga");
    }

    [Fact]
    public async Task Returns_400_BadRequest_When_Input_Ids_Are_Empty()
    {
        var resp = await _client.PostAsJsonAsync("/bookings", new { memberId = Guid.Empty, classId = Guid.Empty });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Can_Book_Detect_Conflict_And_Cancel_Class()
    {
        var members = await _client.GetFromJsonAsync<List<Member>>("/members");
        var activeMember = members!.First(m => m.IsActive);

        var classes = await _client.GetFromJsonAsync<List<ClassAvailability>>("/classes");
        var cls = classes!.First(c => c.Name == "Yoga");

        var bookingReq = new { memberId = activeMember.Id.Value, classId = cls.Id };

        // 1. Create booking -> 201 Created
        var bookResp = await _client.PostAsJsonAsync("/bookings", bookingReq);
        Assert.Equal(HttpStatusCode.Created, bookResp.StatusCode);

        var booking = await bookResp.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(booking);

        // 2. Duplicate booking -> 409 Conflict
        var duplicateResp = await _client.PostAsJsonAsync("/bookings", bookingReq);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResp.StatusCode);

        // 3. Cancel booking -> 204 NoContent
        var cancelResp = await _client.DeleteAsync($"/bookings/{booking!.Id.Value}");
        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);
    }
}
