namespace PastaFit.Core.Domain;

public abstract record BookingError(string Code, string Message)
{
    public sealed record ClassFull() : BookingError("Booking.ClassFull", "Class is already full");
    public sealed record AlreadyBooked() : BookingError("Booking.AlreadyBooked", "Member already booked");
    public sealed record MemberNotFound() : BookingError("Booking.MemberNotFound", "Member not found");
    public sealed record MemberInactive() : BookingError("Booking.MemberInactive", "Member is inactive");
    public sealed record ClassNotFound() : BookingError("Booking.ClassNotFound", "Class not found");
    public sealed record BookingNotFound() : BookingError("Booking.NotFound", "Booking not found");
}