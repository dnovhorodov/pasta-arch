using FunqTypes;
using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.Ports;

public static class BookingPortFactories
{
    public static GetBooking GetBookingPort(List<Core.Domain.Booking> bookings) =>
        bookingId => Task.FromResult(
            bookings.FirstOrDefault(b => b.Id == bookingId) is { } booking
                ? Result<Core.Domain.Booking, BookingError>.Ok(booking)
                : Result<Core.Domain.Booking, BookingError>.Fail(new BookingError.BookingNotFound())
        );
}