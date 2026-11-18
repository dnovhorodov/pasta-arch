namespace PastaFit.Features.Booking.CancelBooking;

public static class CancelBookingEndpoint
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapCancelBooking()
        {
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

            return app;
        }
    }
}
