using FunqTypes;

namespace PastaFit.Core.Domain;

/// <summary>
/// Pure Functional Core: deterministic, synchronous, zero I/O, zero exceptions.
/// </summary>
public static class BookingPolicy
{
    public static Result<Booking, BookingError> Create(
        Member member,
        Class gymClass,
        int activeBookingsCount,
        bool hasExistingBooking,
        DateTimeOffset nowUtc)
    {
        var memberActiveCheck = member.IsActive
            ? Result<Unit, BookingError>.Ok(Unit.Value)
            : Result<Unit, BookingError>.Fail(new BookingError.MemberInactive());

        var notAlreadyBookedCheck = !hasExistingBooking
            ? Result<Unit, BookingError>.Ok(Unit.Value)
            : Result<Unit, BookingError>.Fail(new BookingError.AlreadyBooked());

        var capacityCheck = activeBookingsCount < gymClass.Capacity
            ? Result<Unit, BookingError>.Ok(Unit.Value)
            : Result<Unit, BookingError>.Fail(new BookingError.ClassFull());

        return Result.Combine(memberActiveCheck, notAlreadyBookedCheck, capacityCheck)
            .Map((_, _, _) => new Booking(BookingId.New(), member.Id, gymClass.Id, nowUtc));
    }
}
