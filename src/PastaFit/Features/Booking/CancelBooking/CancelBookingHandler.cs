using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.Ports;

namespace PastaFit.Features.Booking.CancelBooking;

public static class CancelBookingHandler
{
    public static async Task<Result<Core.Domain.Booking, BookingError>> Handle(
        Guid bookingId,
        BookingDependencies deps)
    {
        var bookingResult = await deps.GetBooking(bookingId);
        if (!bookingResult.IsSuccess) return bookingResult;

        await deps.CancelBooking(bookingId);
        return bookingResult;
    }
}