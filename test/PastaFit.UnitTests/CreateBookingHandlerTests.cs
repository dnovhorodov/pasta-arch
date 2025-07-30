using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CreateBooking;
using PastaFit.Features.Booking.Ports;

namespace PastaFit.UnitTests;

public class CreateBookingHandlerTests
{
    [Fact]
    public async Task Fails_If_Member_Not_Found()
    {
        var deps = new BookingDependencies(
            HasExistingBooking: (_, _) => Task.FromResult(false),
            IsClassFull: _ => Task.FromResult(false),
            GetMember: _ => Task.FromResult(Result<Member, BookingError>.Fail(new BookingError.MemberNotFound())),
            GetClass: _ => Task.FromResult(Result<Class, BookingError>.Ok(new(Guid.NewGuid(), "Yoga", 5))),
            SaveBooking: _ => Task.CompletedTask,
            GetBooking: _ => throw new NotImplementedException(),
            CancelBooking: _ => throw new NotImplementedException()
        );

        var result = await CreateBookingHandler.Handle(Guid.NewGuid(), Guid.NewGuid(), deps);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.MemberNotFound);
    }

    [Fact]
    public async Task Fails_If_Class_Is_Full()
    {
        var deps = new BookingDependencies(
            HasExistingBooking: (_, _) => Task.FromResult(false),
            IsClassFull: _ => Task.FromResult(true),
            GetMember: _ => Task.FromResult(Result<Member, BookingError>.Ok(new Member(Guid.NewGuid(), "Alice", true))),
            GetClass: _ => Task.FromResult(Result<Class, BookingError>.Ok(new Class(Guid.NewGuid(), "Yoga", 5))),
            SaveBooking: _ => Task.CompletedTask,
            GetBooking: _ => throw new NotImplementedException(),
            CancelBooking: _ => throw new NotImplementedException()
        );

        var result = await CreateBookingHandler.Handle(Guid.NewGuid(), Guid.NewGuid(), deps);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.ClassFull);
    }
    
    [Fact]
    public async Task Fails_If_Member_Is_Inactive()
    {
        var deps = new BookingDependencies(
            HasExistingBooking: (_, _) => Task.FromResult(false),
            IsClassFull: _ => Task.FromResult(false),
            GetMember: _ => Task.FromResult(Result<Member, BookingError>.Ok(new Member(Guid.NewGuid(), "Bob", false))),
            GetClass: _ => Task.FromResult(Result<Class, BookingError>.Ok(new Class(Guid.NewGuid(), "Yoga", 5))),
            SaveBooking: _ => Task.CompletedTask,
            GetBooking: _ => throw new NotImplementedException(),
            CancelBooking: _ => throw new NotImplementedException()
        );

        var result = await CreateBookingHandler.Handle(Guid.NewGuid(), Guid.NewGuid(), deps);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.MemberInactive);
    }

    [Fact]
    public async Task Fails_If_Already_Booked()
    {
        var deps = new BookingDependencies(
            HasExistingBooking: (_, _) => Task.FromResult(true),
            IsClassFull: _ => Task.FromResult(false),
            GetMember: _ => Task.FromResult(Result<Member, BookingError>.Ok(new Member(Guid.NewGuid(), "Charlie", true))),
            GetClass: _ => Task.FromResult(Result<Class, BookingError>.Ok(new Class(Guid.NewGuid(), "Yoga", 5))),
            SaveBooking: _ => Task.CompletedTask,
            GetBooking: _ => throw new NotImplementedException(),
            CancelBooking: _ => throw new NotImplementedException()
        );

        var result = await CreateBookingHandler.Handle(Guid.NewGuid(), Guid.NewGuid(), deps);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e is BookingError.AlreadyBooked);
    }
}