using Dapper;
using FunqTypes;
using Npgsql;
using PastaFit.Core.Domain;
using PastaFit.Features.Booking.CancelBooking;
using PastaFit.Features.Booking.CreateBooking;
using PastaFit.Features.Booking.Ports;

namespace PastaFit.Features.Booking.Adapters;

public static class PostgresBookingAdapter
{
    public static async Task InitializeSchemaAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS classes (
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                capacity INT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS members (
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                is_active BOOLEAN NOT NULL
            );
            CREATE TABLE IF NOT EXISTS bookings (
                id UUID PRIMARY KEY,
                member_id UUID NOT NULL REFERENCES members(id),
                class_id UUID NOT NULL REFERENCES classes(id),
                created_at_utc TIMESTAMPTZ NOT NULL
            );
            """);

        foreach (var cls in DefaultSeedData.Classes)
        {
            await conn.ExecuteAsync(
                "INSERT INTO classes (id, name, capacity) VALUES (@Id, @Name, @Capacity) ON CONFLICT (id) DO NOTHING;",
                new { Id = cls.Id.Value, cls.Name, cls.Capacity });
        }

        foreach (var member in DefaultSeedData.Members)
        {
            await conn.ExecuteAsync(
                "INSERT INTO members (id, name, is_active) VALUES (@Id, @Name, @IsActive) ON CONFLICT (id) DO NOTHING;",
                new { Id = member.Id.Value, member.Name, member.IsActive });
        }
    }

    public static CreateBookingPorts CreateCreateBookingPorts(NpgsqlDataSource dataSource) => new(
        GetMember: async (memberId, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            var row = await conn.QuerySingleOrDefaultAsync<MemberRow>(
                "SELECT id AS Id, name AS Name, is_active AS IsActive FROM members WHERE id = @Id;",
                new { Id = memberId.Value });
            return row is not null
                ? Option<Member>.Some(new Member(new MemberId(row.Id), row.Name, row.IsActive))
                : Option<Member>.None();
        },

        GetClass: async (classId, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            var row = await conn.QuerySingleOrDefaultAsync<ClassRow>(
                "SELECT id AS Id, name AS Name, capacity AS Capacity FROM classes WHERE id = @Id;",
                new { Id = classId.Value });
            return row is not null
                ? Option<Class>.Some(new Class(new ClassId(row.Id), row.Name, row.Capacity))
                : Option<Class>.None();
        },

        CountClassBookings: async (classId, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM bookings WHERE class_id = @ClassId;",
                new { ClassId = classId.Value });
        },

        HasExistingBooking: async (memberId, classId, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            return await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM bookings WHERE member_id = @MemberId AND class_id = @ClassId);",
                new { MemberId = memberId.Value, ClassId = classId.Value });
        },

        SaveBooking: async (booking, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            await conn.ExecuteAsync(
                "INSERT INTO bookings (id, member_id, class_id, created_at_utc) VALUES (@Id, @MemberId, @ClassId, @CreatedAtUtc);",
                new
                {
                    Id = booking.Id.Value,
                    MemberId = booking.MemberId.Value,
                    ClassId = booking.ClassId.Value,
                    booking.CreatedAtUtc
                });
            return Result<Unit, BookingError>.Ok(Unit.Value);
        }
    );

    public static CancelBookingPorts CreateCancelBookingPorts(NpgsqlDataSource dataSource) => new(
        GetBooking: async (bookingId, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            var row = await conn.QuerySingleOrDefaultAsync<BookingRow>(
                "SELECT id AS Id, member_id AS MemberId, class_id AS ClassId, created_at_utc AS CreatedAtUtc FROM bookings WHERE id = @Id;",
                new { Id = bookingId.Value });
            return row is not null
                ? Option<Core.Domain.Booking>.Some(new Core.Domain.Booking(
                    new BookingId(row.Id),
                    new MemberId(row.MemberId),
                    new ClassId(row.ClassId),
                    row.CreatedAtUtc))
                : Option<Core.Domain.Booking>.None();
        },

        DeleteBooking: async (bookingId, ct) =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            await conn.ExecuteAsync("DELETE FROM bookings WHERE id = @Id;", new { Id = bookingId.Value });
            return Result<Unit, BookingError>.Ok(Unit.Value);
        }
    );

    public static BookingQueryPorts CreateQueryPorts(NpgsqlDataSource dataSource) => new(
        GetClassAvailability: async ct =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            var rows = await conn.QueryAsync<ClassAvailability>("""
                SELECT c.id AS Id,
                       c.name AS Name,
                       c.capacity AS Capacity,
                       (c.capacity - COUNT(b.id)::int) AS Available
                FROM classes c
                LEFT JOIN bookings b ON b.class_id = c.id
                GROUP BY c.id, c.name, c.capacity
                ORDER BY c.name;
                """);
            return rows.ToList();
        },

        GetAllMembers: async ct =>
        {
            await using var conn = await dataSource.OpenConnectionAsync(ct);
            var rows = await conn.QueryAsync<MemberRow>(
                "SELECT id AS Id, name AS Name, is_active AS IsActive FROM members ORDER BY name;");
            return rows.Select(r => new Member(new MemberId(r.Id), r.Name, r.IsActive)).ToList();
        }
    );

    private sealed record MemberRow(Guid Id, string Name, bool IsActive);
    private sealed record ClassRow(Guid Id, string Name, int Capacity);
    private sealed record BookingRow(Guid Id, Guid MemberId, Guid ClassId, DateTimeOffset CreatedAtUtc);
}
