using FunqTypes;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CancelBooking;
using PastaFit.Features.Booking.CreateBooking;
using PastaFit.Features.Booking.Ports;

namespace PastaFit.Features.Booking.Adapters;

public sealed class InMemoryBookingAdapter
{
    private readonly List<Core.Domain.Booking> _bookings = [];
    private readonly Dictionary<ClassId, Class> _classes = [];
    private readonly Dictionary<MemberId, Member> _members = [];

    public InMemoryBookingAdapter()
    {
        foreach (var cls in DefaultSeedData.Classes)
            _classes[cls.Id] = cls;

        foreach (var member in DefaultSeedData.Members)
            _members[member.Id] = member;
    }

    public CreateBookingPorts CreateCreateBookingPorts() => new(
        GetMember: (memberId, _) =>
            ValueTask.FromResult(_members.TryGetValue(memberId, out var member)
                ? Option<Member>.Some(member)
                : Option<Member>.None()),

        GetClass: (classId, _) =>
            ValueTask.FromResult(_classes.TryGetValue(classId, out var cls)
                ? Option<Class>.Some(cls)
                : Option<Class>.None()),

        CountClassBookings: (classId, _) =>
            ValueTask.FromResult(_bookings.Count(b => b.ClassId == classId)),

        HasExistingBooking: (memberId, classId, _) =>
            ValueTask.FromResult(_bookings.Any(b => b.MemberId == memberId && b.ClassId == classId)),

        SaveBooking: (booking, _) =>
        {
            _bookings.Add(booking);
            return ValueTask.FromResult(Result<Unit, BookingError>.Ok(Unit.Value));
        }
    );

    public CancelBookingPorts CreateCancelBookingPorts() => new(
        GetBooking: (bookingId, _) =>
            ValueTask.FromResult(_bookings.FirstOrDefault(b => b.Id == bookingId).ToOption()),

        DeleteBooking: (bookingId, _) =>
        {
            _bookings.RemoveAll(b => b.Id == bookingId);
            return ValueTask.FromResult(Result<Unit, BookingError>.Ok(Unit.Value));
        }
    );

    public BookingQueryPorts CreateQueryPorts() => new(
        GetClassAvailability: _ =>
            ValueTask.FromResult<IReadOnlyList<ClassAvailability>>(_classes.Values
                .Select(cls => new ClassAvailability(
                    cls.Id.Value,
                    cls.Name,
                    cls.Capacity,
                    cls.Capacity - _bookings.Count(b => b.ClassId == cls.Id)))
                .ToList()),

        GetAllMembers: _ =>
            ValueTask.FromResult<IReadOnlyList<Member>>(_members.Values.ToList())
    );
}

public static class DefaultSeedData
{
    public static readonly IReadOnlyList<Class> Classes =
    [
        new(new ClassId(Guid.Parse("11111111-1111-1111-1111-111111111111")), "Yoga", 5),
        new(new ClassId(Guid.Parse("22222222-2222-2222-2222-222222222222")), "Spin", 2)
    ];

    public static readonly IReadOnlyList<Member> Members =
    [
        new(new MemberId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")), "Alice", true),
        new(new MemberId(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")), "Bob", false),
        new(new MemberId(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")), "Maggy", true),
        new(new MemberId(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd")), "Chuck", true)
    ];
}
