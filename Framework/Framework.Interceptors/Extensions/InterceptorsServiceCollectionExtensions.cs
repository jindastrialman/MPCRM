using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Framework.Interceptors.Services;
using Framework.Interceptors.Abstractions;
using Framework.Interceptors.Common;
using Framework.DataAccess.Models;

namespace Framework.Interceptors.Extensions;

public static class InterceptorsServiceCollectionExtensions
{
    public static IServiceCollection AddInterceptorService<T>(this IServiceCollection services, IConfiguration configuration) where T : IBaseEntity
        {
            services.AddScoped<IRequestInterceptorService<T>, RequestInterceptorService<T>>();

            services.AddScoped<IRequestInterceptor<T>, CreatedAtInterceptor<T>>();
            services.AddScoped<IRequestInterceptor<T>, HardDeleteInterceptor<T>>();
            services.AddScoped<IRequestInterceptor<T>, UpdatedAtInterceptor<T>>();

            return services; 
        }
}
