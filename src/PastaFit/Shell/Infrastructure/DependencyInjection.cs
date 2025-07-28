namespace PastaFit.Shell.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingUseCases(this IServiceCollection services)
    {
        InMemoryBookingData.Bootstrap();
        services.AddSingleton(InMemoryBookingData.GetBookingDependencies());
        
        return services;
    }
}