using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CancelBooking;

namespace PastaFit.UnitTests;

public class CancelBookingHandlerTests
{
    [Fact]
    public async Task Fails_If_Booking_Not_Found()
    {
        var ports = new CancelBookingPorts(
            GetBooking: (_, _) => ValueTask.FromResult(Option<Booking>.None()),
            DeleteBooking: (_, _) => ValueTask.FromResult(Result<Unit, BookingError>.Ok(Unit.Value))
        );

        var result = await CancelBookingHandler.Handle(Guid.NewGuid(), ports);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.BookingNotFound);
    }

    [Fact]
    public async Task Succeeds_When_Booking_Exists()
    {
        var bookingId = BookingId.New();
        var booking = new Booking(bookingId, MemberId.New(), ClassId.New(), DateTimeOffset.UtcNow);
        var wasDeleted = false;

        var ports = new CancelBookingPorts(
            GetBooking: (_, _) => ValueTask.FromResult(Option<Booking>.Some(booking)),
            DeleteBooking: (_, _) =>
            {
                wasDeleted = true;
                return ValueTask.FromResult(Result<Unit, BookingError>.Ok(Unit.Value));
            }
        );

        var result = await CancelBookingHandler.Handle(bookingId.Value, ports);

        Assert.True(result.IsSuccess);
        Assert.Equal(bookingId, result.Value.Id);
        Assert.True(wasDeleted);
    }
}
