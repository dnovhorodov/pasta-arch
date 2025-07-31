using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CancelBooking;

namespace PastaFit.UnitTests;

public class CancelBookingHandlerTests
{
    [Fact]
    public async Task Fails_If_Booking_Not_Found()
    {
        var deps = new CancelBookingDependencies(
            GetBooking: _ => Task.FromResult(Result<Booking, BookingError>.Fail(new BookingError.BookingNotFound())),
            CancelBooking: _ => Task.CompletedTask
        );

        var result = await CancelBookingHandler.Handle(Guid.NewGuid(), deps);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.BookingNotFound);
    }

    [Fact]
    public async Task Succeeds_When_Booking_Exists()
    {
        var bookingId = Guid.NewGuid();
        var booking = new Booking(bookingId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        var wasCancelled = false;

        var deps = new CancelBookingDependencies(
            GetBooking: _ => Task.FromResult(Result<Booking, BookingError>.Ok(booking)),
            CancelBooking: id =>
            {
                wasCancelled = true;
                return Task.CompletedTask;
            }
        );

        var result = await CancelBookingHandler.Handle(bookingId, deps);

        Assert.True(result.IsSuccess);
        Assert.Equal(bookingId, result.Value.Id);
        Assert.True(wasCancelled);
    }
}