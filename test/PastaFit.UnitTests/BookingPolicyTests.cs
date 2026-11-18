using PastaFit.Core.Domain;

namespace PastaFit.UnitTests;

public class BookingPolicyTests
{
    private static readonly Member ActiveMember = new(MemberId.New(), "Alice", true);
    private static readonly Member InactiveMember = new(MemberId.New(), "Bob", false);
    private static readonly Class YogaClass = new(ClassId.New(), "Yoga", 5);

    [Fact]
    public void Create_WhenValid_ReturnsBooking()
    {
        var now = DateTimeOffset.UtcNow;

        var result = BookingPolicy.Create(ActiveMember, YogaClass, activeBookingsCount: 2, hasExistingBooking: false, now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ActiveMember.Id, result.Value.MemberId);
        Assert.Equal(YogaClass.Id, result.Value.ClassId);
        Assert.Equal(now, result.Value.CreatedAtUtc);
    }

    [Fact]
    public void Create_WhenMultipleRulesViolated_AccumulatesAllErrorsViaCombine()
    {
        var result = BookingPolicy.Create(
            InactiveMember,
            YogaClass,
            activeBookingsCount: 5,
            hasExistingBooking: true,
            DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e is BookingError.MemberInactive);
        Assert.Contains(result.Errors, e => e is BookingError.AlreadyBooked);
        Assert.Contains(result.Errors, e => e is BookingError.ClassFull);
    }
}
