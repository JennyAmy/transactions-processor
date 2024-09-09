namespace Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using TransactionsProcessor.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<RedisService>();

        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true
        );

        return services;
    }
}
