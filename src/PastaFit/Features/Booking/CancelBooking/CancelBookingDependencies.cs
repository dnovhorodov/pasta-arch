using PastaFit.Features.Booking.Ports;

namespace PastaFit.Features.Booking.CancelBooking;

public sealed record CancelBookingDependencies(
    GetBooking GetBooking,
    Ports.CancelBooking CancelBooking
);