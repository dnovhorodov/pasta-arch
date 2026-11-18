using FunqTypes;
using PastaFit.Core.Domain;

namespace PastaFit.Features.Booking.CancelBooking;

public static class CancelBookingHandler
{
    public static async Task<Result<Core.Domain.Booking, BookingError>> Handle(
        Guid rawBookingId,
        CancelBookingPorts ports,
        CancellationToken ct = default)
    {
        return await BookingId.From(rawBookingId)
            .BindAsync(async bookingId =>
                (await ports.GetBooking(bookingId, ct))
                    .ToResult<BookingError>(new BookingError.BookingNotFound()))
            .BindAsync(async booking =>
                (await ports.DeleteBooking(booking.Id, ct)).Map(_ => booking));
    }
}
