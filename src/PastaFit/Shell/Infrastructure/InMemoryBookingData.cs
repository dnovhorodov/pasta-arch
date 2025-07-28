using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.Contracts;

namespace PastaFit.Shell.Infrastructure;

public static class InMemoryBookingData
{
    private static readonly List<Booking> Bookings = new();
    private static readonly Dictionary<Guid, Class> Classes = new();
    private static readonly Dictionary<Guid, Member> Members = new();

    public static void Bootstrap()
    {
        var yoga = new Class(Guid.NewGuid(), "Yoga", 5);
        var spin = new Class(Guid.NewGuid(), "Spin", 3);

        Classes[yoga.Id] = yoga;
        Classes[spin.Id] = spin;

        var alice = new Member(Guid.NewGuid(), "Alice", true);
        var bob = new Member(Guid.NewGuid(), "Bob", false);
        var john = new Member(Guid.NewGuid(), "John", false);
        var maggy = new Member(Guid.NewGuid(), "Maggy", true);
        var chuck = new Member(Guid.NewGuid(), "Chuck", true);
        var julia = new Member(Guid.NewGuid(), "Julia", true);

        Members[alice.Id] = alice;
        Members[bob.Id] = bob;
        Members[john.Id] = john;
        Members[maggy.Id] = maggy;
        Members[chuck.Id] = chuck;
        Members[julia.Id] = julia;
    }

    public static BookingDependencies GetBookingDependencies() => new(
        HasExistingBooking: (memberId, classId) =>
            Task.FromResult(Bookings.Any(b => b.MemberId == memberId && b.ClassId == classId)),

        IsClassFull: classId =>
        {
            if (!Classes.TryGetValue(classId, out var cls)) return Task.FromResult(true);
            var bookings = Bookings.Count(b => b.ClassId == classId);
            return Task.FromResult(bookings >= cls.Capacity);
        },

        GetMember: memberId =>
            Task.FromResult(Members.TryGetValue(memberId, out var member)
                ? Result<Member, BookingError>.Ok(member)
                : Result<Member, BookingError>.Fail(new BookingError.MemberNotFound())),

        GetClass: classId =>
            Task.FromResult(Classes.TryGetValue(classId, out var cls)
                ? Result<Class, BookingError>.Ok(cls)
                : Result<Class, BookingError>.Fail(new BookingError.ClassNotFound())),

        SaveBooking: booking =>
        {
            Bookings.Add(booking);
            return Task.CompletedTask;
        },

        GetBooking: bookingId =>
            Task.FromResult(Bookings.FirstOrDefault(b => b.Id == bookingId) is { } booking
                ? Result<Booking, BookingError>.Ok(booking)
                : Result<Booking, BookingError>.Fail(new BookingError.BookingNotFound())),

        CancelBooking: bookingId =>
        {
            Bookings.RemoveAll(b => b.Id == bookingId);
            return Task.CompletedTask;
        }
    );

    public static IEnumerable<object> GetClassAvailability() => Classes.Values.Select(cls => new
    {
        cls.Id,
        cls.Name,
        cls.Capacity,
        Available = cls.Capacity - Bookings.Count(b => b.ClassId == cls.Id)
    });
    
    public static IEnumerable<Member> GetAllMembers() => Members.Values;
}
