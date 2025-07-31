using FunqTypes;
using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.Ports;

public delegate Task<bool> HasExistingBooking(Guid memberId, Guid classId);
public delegate Task<bool> IsClassFull(Guid classId);
public delegate Task<Result<Member, BookingError>> GetMember(Guid memberId);
public delegate Task<Result<Class, BookingError>> GetClass(Guid classId);
public delegate Task SaveBooking(Core.Domain.Booking booking);
public delegate Task<Result<Core.Domain.Booking, BookingError>> GetBooking(Guid bookingId);
public delegate Task CancelBooking(Guid bookingId);