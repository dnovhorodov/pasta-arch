namespace PastaFit.Features.Booking.CreateBooking;

public sealed record CreateBookingRequest(Guid MemberId, Guid ClassId);

public static class CreateBookingEndpoint
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapCreateBooking()
        {
            app.MapPost("/bookings", async (
                CreateBookingRequest request,
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

            return app;
        }
    }
}
