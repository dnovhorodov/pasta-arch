namespace PastaFit.Core.Domain;

public sealed record Booking(BookingId Id, MemberId MemberId, ClassId ClassId, DateTimeOffset CreatedAtUtc);
