using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CancelBooking;
using PastaFit.Features.Booking.CreateBooking;
using PastaFit.Features.Booking.Ports;

namespace PastaFit.Shell.Endpoints;

public static class BookingEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapBookingEndpoints()
        {
            app.MapPost("/bookings", async (
                BookingRequest request,
                CreateBookingPorts ports,
                TimeProvider timeProvider,
                CancellationToken ct) =>
            {
                var result = await CreateBookingHandler.Handle(
                    new CreateBookingCommand(request.MemberId, request.ClassId),
                    ports,
                    timeProvider,
                    ct);

                return result.Match(
                    booking => Results.Created($"/bookings/{booking.Id.Value}", booking),
                    errors => errors.ToHttpResult()
                );
            });

            app.MapDelete("/bookings/{bookingId:guid}", async (
                Guid bookingId,
                CancelBookingPorts ports,
                CancellationToken ct) =>
            {
                var result = await CancelBookingHandler.Handle(bookingId, ports, ct);
                return result.Match(
                    _ => Results.NoContent(),
                    errors => errors.ToHttpResult()
                );
            });

            app.MapGet("/classes", async (BookingQueryPorts queryPorts, CancellationToken ct) =>
                Results.Ok(await queryPorts.GetClassAvailability(ct)));

            app.MapGet("/members", async (BookingQueryPorts queryPorts, CancellationToken ct) =>
                Results.Ok(await queryPorts.GetAllMembers(ct)));

            return app;
        }
    }

    extension(IReadOnlyList<BookingError> errors)
    {
        private IResult ToHttpResult() =>
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

public sealed record BookingRequest(Guid MemberId, Guid ClassId);
