using PastaFit.Shell.Endpoints;
using PastaFit.Shell.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBookingUseCases();

var app = builder.Build();

app.MapBookingEndpoints();

app.Run();
