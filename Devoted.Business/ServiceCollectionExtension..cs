using Microsoft.Extensions.DependencyInjection;

namespace Devoted.Business
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterBusinessServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
