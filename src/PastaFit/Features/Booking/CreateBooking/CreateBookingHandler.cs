using FunqTypes;
using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.CreateBooking;

public sealed record CreateBookingCommand(Guid MemberId, Guid ClassId);

public static class CreateBookingHandler
{
    public static async Task<Result<Core.Domain.Booking, BookingError>> Handle(
        CreateBookingCommand command,
        CreateBookingPorts ports,
        TimeProvider timeProvider,
        CancellationToken ct = default)
    {
        // 1. Validate & parse raw inputs into Typed Abstractions (accumulating any errors via Result.Combine)
        var inputResult = Result.Combine(
            MemberId.From(command.MemberId),
            ClassId.From(command.ClassId));

        // 2. Railway pipeline: Load state via Ports -> Pure Domain Policy -> Save via Port
        return await inputResult
            .BindAsync(async (memberId, classId) =>
            {
                var memberResult = (await ports.GetMember(memberId, ct))
                    .ToResult<BookingError>(new BookingError.MemberNotFound());
                var classResult = (await ports.GetClass(classId, ct))
                    .ToResult<BookingError>(new BookingError.ClassNotFound());

                return await Result.Combine(memberResult, classResult)
                    .BindAsync(async (member, gymClass) =>
                    {
                        var activeCount = await ports.CountClassBookings(classId, ct);
                        var alreadyBooked = await ports.HasExistingBooking(memberId, classId, ct);

                        return BookingPolicy.Create(
                            member,
                            gymClass,
                            activeCount,
                            alreadyBooked,
                            timeProvider.GetUtcNow());
                    });
            })
            .BindAsync(async booking =>
                (await ports.SaveBooking(booking, ct)).Map(_ => booking));
    }
}
