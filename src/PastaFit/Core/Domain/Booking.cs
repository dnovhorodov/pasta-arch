namespace PastaFit.Core.Domain;

public sealed record Booking(Guid Id, Guid MemberId, Guid ClassId, DateTime Time);