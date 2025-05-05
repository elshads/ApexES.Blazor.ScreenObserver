using Microsoft.Extensions.DependencyInjection;

namespace ApexES.Blazor.ScreenObserver;

public static class ServiceExtensions
{
    public static IServiceCollection AddScreenObserver(this IServiceCollection services)
    {
        services.AddScoped<IBrowserScreenService, BrowserScreenService>();

        return services;
    }
}