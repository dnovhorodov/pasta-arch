using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.Contracts;
using static FunqTypes.Result<PastaFit.Core.Domain.Booking, PastaFit.Core.Domain.BookingError>;

namespace PastaFit.Features.Booking.CreateBooking;

public static class CreateBookingHandler
{
    public static async Task<Result<Core.Domain.Booking, BookingError>> Handle(
        Guid memberId,
        Guid classId,
        BookingDependencies deps)
    {
        var memberResult = await deps.GetMember(memberId);
        if (!memberResult.IsSuccess)
            return Fail(memberResult.Errors.ToArray());

        if (!memberResult.Value.IsActive)
            return Fail(new BookingError.MemberInactive());

        var classResult = await deps.GetClass(classId);
        if (!classResult.IsSuccess)
            return Fail(classResult.Errors.ToArray());

        if (await deps.HasExistingBooking(memberId, classId))
            return Fail(new BookingError.AlreadyBooked());

        if (await deps.IsClassFull(classId))
            return Fail(new BookingError.ClassFull());

        var booking = new Core.Domain.Booking(Guid.NewGuid(), memberId, classId, DateTime.UtcNow);
        await deps.SaveBooking(booking);

        return booking;
    }
}