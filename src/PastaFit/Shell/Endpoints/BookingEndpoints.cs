using PastaFit.Features.Booking.CancelBooking;
using PastaFit.Features.Booking.Contracts;
using PastaFit.Features.Booking.CreateBooking;
using PastaFit.Shell.Infrastructure;

namespace PastaFit.Shell.Endpoints;

public static class BookingEndpoints
{
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/bookings", async (
            BookingRequest request,
            BookingDependencies deps) =>
        {
            var result = await CreateBookingHandler.Handle(request.MemberId, request.ClassId, deps);
            return result.Match(
                booking => Results.Created($"/bookings/{booking.Id}", booking),
                errors => Results.BadRequest(errors)
            );
        });
        
        app.MapDelete("/bookings/{bookingId:guid}", async (
            Guid bookingId,
            BookingDependencies deps) =>
        {
            var result = await CancelBookingHandler.Handle(bookingId, deps);
            return result.Match(
                _ => Results.NoContent(),
                errors => Results.NotFound(errors)
            );
        });

        app.MapGet("/classes", () => InMemoryBookingData.GetClassAvailability());
        
        app.MapGet("/members", () => InMemoryBookingData.GetAllMembers());


        return app;
    }
}

public sealed record BookingRequest(Guid MemberId, Guid ClassId);