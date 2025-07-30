using PastaFit.Features.Booking.Adapters;

namespace PastaFit.Shell.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingUseCases(this IServiceCollection services)
    {
        InMemoryBookingAdapter.Bootstrap();
        services.AddSingleton(InMemoryBookingAdapter.GetBookingDependencies());
        
        return services;
    }
}