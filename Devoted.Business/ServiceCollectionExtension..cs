using Devoted.Business.Interfaces;
using Devoted.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Devoted.Business
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
