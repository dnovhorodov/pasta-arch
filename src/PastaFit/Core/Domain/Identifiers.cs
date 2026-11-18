using System.Text.Json;
using System.Text.Json.Serialization;
using FunqTypes;

namespace PastaFit.Core.Domain;

[JsonConverter(typeof(BookingIdJsonConverter))]
public readonly record struct BookingId(Guid Value)
{
    public static BookingId New() => new(Guid.NewGuid());

    public static Result<BookingId, BookingError> From(Guid value) =>
        value == Guid.Empty
            ? Result<BookingId, BookingError>.Fail(new BookingError.InvalidInput(nameof(BookingId), "BookingId cannot be empty"))
            : Result<BookingId, BookingError>.Ok(new BookingId(value));
}

[JsonConverter(typeof(MemberIdJsonConverter))]
public readonly record struct MemberId(Guid Value)
{
    public static MemberId New() => new(Guid.NewGuid());

    public static Result<MemberId, BookingError> From(Guid value) =>
        value == Guid.Empty
            ? Result<MemberId, BookingError>.Fail(new BookingError.InvalidInput(nameof(MemberId), "MemberId cannot be empty"))
            : Result<MemberId, BookingError>.Ok(new MemberId(value));
}

[JsonConverter(typeof(ClassIdJsonConverter))]
public readonly record struct ClassId(Guid Value)
{
    public static ClassId New() => new(Guid.NewGuid());

    public static Result<ClassId, BookingError> From(Guid value) =>
        value == Guid.Empty
            ? Result<ClassId, BookingError>.Fail(new BookingError.InvalidInput(nameof(ClassId), "ClassId cannot be empty"))
            : Result<ClassId, BookingError>.Ok(new ClassId(value));
}

public sealed class BookingIdJsonConverter : JsonConverter<BookingId>
{
    public override BookingId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, BookingId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class MemberIdJsonConverter : JsonConverter<MemberId>
{
    public override MemberId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, MemberId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class ClassIdJsonConverter : JsonConverter<ClassId>
{
    public override ClassId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, ClassId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}
