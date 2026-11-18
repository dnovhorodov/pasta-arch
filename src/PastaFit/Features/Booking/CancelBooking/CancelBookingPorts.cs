using FunqTypes;
using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.CancelBooking;

public sealed record CancelBookingPorts(
    Func<BookingId, CancellationToken, ValueTask<Option<Core.Domain.Booking>>> GetBooking,
    Func<BookingId, CancellationToken, ValueTask<Result<Unit, BookingError>>> DeleteBooking
);
