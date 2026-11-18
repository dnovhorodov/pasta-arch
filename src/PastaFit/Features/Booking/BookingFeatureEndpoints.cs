using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CancelBooking;
using PastaFit.Features.Booking.CreateBooking;
using PastaFit.Features.Booking.Ports;

namespace PastaFit.Features.Booking;

public static class BookingFeatureEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBookingEndpoints()
        {
            app.MapCreateBooking();
            app.MapCancelBooking();

            app.MapGet("/classes", async (BookingQueryPorts queryPorts, CancellationToken ct) =>
                Results.Ok(await queryPorts.GetClassAvailability(ct)));

            app.MapGet("/members", async (BookingQueryPorts queryPorts, CancellationToken ct) =>
                Results.Ok(await queryPorts.GetAllMembers(ct)));

            return app;
        }
    }

    extension(IReadOnlyList<BookingError> errors)
    {
        public IResult ToHttpResult() =>
            errors[0] switch
            {
                BookingError.InvalidInput => Results.BadRequest(errors),
                BookingError.MemberNotFound or BookingError.ClassNotFound or BookingError.BookingNotFound => Results.NotFound(errors),
                BookingError.ClassFull or BookingError.AlreadyBooked => Results.Conflict(errors),
                BookingError.MemberInactive => Results.UnprocessableEntity(errors),
                _ => Results.BadRequest(errors)
            };
    }
}
