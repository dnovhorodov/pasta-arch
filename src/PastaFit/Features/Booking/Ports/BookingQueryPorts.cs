using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.Ports;

public sealed record BookingQueryPorts(
    Func<CancellationToken, ValueTask<IReadOnlyList<ClassAvailability>>> GetClassAvailability,
    Func<CancellationToken, ValueTask<IReadOnlyList<Member>>> GetAllMembers
);
