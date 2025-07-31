using PastaFit.Features.Booking.Ports;

namespace PastaFit.Features.Booking.CreateBooking;

public sealed record CreateBookingDependencies(
    HasExistingBooking HasExistingBooking,
    IsClassFull IsClassFull,
    GetMember GetMember,
    GetClass GetClass,
    SaveBooking SaveBooking,
    GetBooking GetBooking
);