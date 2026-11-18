using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CreateBooking;

namespace PastaFit.UnitTests;

public class CreateBookingHandlerTests
{
    private static CreateBookingPorts CreatePorts(
        Option<Member>? member = null,
        Option<Class>? gymClass = null,
        int activeCount = 0,
        bool alreadyBooked = false,
        Action<Booking>? onSave = null) => new(
        GetMember: (id, _) => ValueTask.FromResult(member ?? Option<Member>.Some(new Member(id, "Alice", true))),
        GetClass: (id, _) => ValueTask.FromResult(gymClass ?? Option<Class>.Some(new Class(id, "Yoga", 5))),
        CountClassBookings: (_, _) => ValueTask.FromResult(activeCount),
        HasExistingBooking: (_, _, _) => ValueTask.FromResult(alreadyBooked),
        SaveBooking: (booking, _) =>
        {
            onSave?.Invoke(booking);
            return ValueTask.FromResult(Result<Unit, BookingError>.Ok(Unit.Value));
        }
    );

    [Fact]
    public async Task Fails_WithAccumulatedErrors_WhenBothInputIdsAreEmpty()
    {
        var ports = CreatePorts();

        var result = await CreateBookingHandler.Handle(
            new CreateBookingCommand(Guid.Empty, Guid.Empty),
            ports,
            TimeProvider.System);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.IsType<BookingError.InvalidInput>(e));
    }

    [Fact]
    public async Task Fails_WithAccumulatedErrors_WhenMemberAndClassNotFound()
    {
        var ports = CreatePorts(
            member: Option<Member>.None(),
            gymClass: Option<Class>.None());

        var result = await CreateBookingHandler.Handle(
            new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid()),
            ports,
            TimeProvider.System);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.MemberNotFound);
        Assert.Contains(result.Errors, e => e is BookingError.ClassNotFound);
    }

    [Fact]
    public async Task Fails_If_Class_Is_Full()
    {
        var ports = CreatePorts(activeCount: 5);

        var result = await CreateBookingHandler.Handle(
            new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid()),
            ports,
            TimeProvider.System);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.ClassFull);
    }

    [Fact]
    public async Task Fails_If_Member_Is_Inactive()
    {
        var memberId = MemberId.New();
        var ports = CreatePorts(member: Option<Member>.Some(new Member(memberId, "Bob", false)));

        var result = await CreateBookingHandler.Handle(
            new CreateBookingCommand(memberId.Value, Guid.NewGuid()),
            ports,
            TimeProvider.System);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.MemberInactive);
    }

    [Fact]
    public async Task Fails_If_Already_Booked()
    {
        var ports = CreatePorts(alreadyBooked: true);

        var result = await CreateBookingHandler.Handle(
            new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid()),
            ports,
            TimeProvider.System);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.AlreadyBooked);
    }

    [Fact]
    public async Task Succeeds_And_Saves_When_Valid()
    {
        Booking? savedBooking = null;
        var ports = CreatePorts(onSave: b => savedBooking = b);

        var result = await CreateBookingHandler.Handle(
            new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid()),
            ports,
            TimeProvider.System);

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedBooking);
        Assert.Equal(result.Value.Id, savedBooking!.Id);
    }
}
