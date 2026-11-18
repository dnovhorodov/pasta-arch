using FunqTypes;
using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.CreateBooking;

public sealed record CreateBookingPorts(
    Func<MemberId, CancellationToken, ValueTask<Option<Member>>> GetMember,
    Func<ClassId, CancellationToken, ValueTask<Option<Class>>> GetClass,
    Func<ClassId, CancellationToken, ValueTask<int>> CountClassBookings,
    Func<MemberId, ClassId, CancellationToken, ValueTask<bool>> HasExistingBooking,
    Func<Core.Domain.Booking, CancellationToken, ValueTask<Result<Unit, BookingError>>> SaveBooking
);
