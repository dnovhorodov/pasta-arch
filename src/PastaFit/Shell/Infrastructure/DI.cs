using Npgsql;
using PastaFit.Features.Booking.Adapters;

namespace PastaFit.Shell.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBookingUseCases(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);

            var postgresConnString = configuration.GetConnectionString("Postgres");
            if (!string.IsNullOrWhiteSpace(postgresConnString))
            {
                var dataSource = NpgsqlDataSource.Create(postgresConnString);
                services.AddSingleton(dataSource);
                services.AddSingleton(PostgresBookingAdapter.CreateCreateBookingPorts(dataSource));
                services.AddSingleton(PostgresBookingAdapter.CreateCancelBookingPorts(dataSource));
                services.AddSingleton(PostgresBookingAdapter.CreateQueryPorts(dataSource));
            }
            else
            {
                var inMemory = new InMemoryBookingAdapter();
                services.AddSingleton(inMemory);
                services.AddSingleton(inMemory.CreateCreateBookingPorts());
                services.AddSingleton(inMemory.CreateCancelBookingPorts());
                services.AddSingleton(inMemory.CreateQueryPorts());
            }

            return services;
        }
    }
}
