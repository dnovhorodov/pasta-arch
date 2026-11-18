using Npgsql;
using PastaFit.Features.Booking;
using PastaFit.Features.Booking.Adapters;
using PastaFit.Shell.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBookingUseCases(builder.Configuration);

var app = builder.Build();

if (app.Services.GetService<NpgsqlDataSource>() is { } dataSource)
{
    await PostgresBookingAdapter.InitializeSchemaAsync(dataSource);
}

app.MapBookingEndpoints();

app.Run();

public partial class Program { }
