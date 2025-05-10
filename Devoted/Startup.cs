using System.Net.Http.Headers;
using Devoted.Business;
using Devoted.Business.Interfaces;
using Devoted.Business.Services;
using Devoted.Persistence;
using Polly;
using Polly.Extensions.Http;

namespace Devoted.API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config) => _config = config;

        public void ConfigureServices(IServiceCollection services)
        {
            // 1) Persistence + Business
            services.RegisterPersistenceServices(_config);
            services.RegisterBusinessServices();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // 2) Typed HttpClient for Product Service with Polly policies
            services.AddHttpClient<IProductClient, ProductClient>(client =>
            {
                client.BaseAddress = new Uri(_config["Services:ProductUrl"]);
                client.DefaultRequestHeaders.Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500)))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(10)));

            // 3) MVC + Swagger
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 4) Serilog already configured in Program.cs via Host.UseSerilog()

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<Devoted.API.Middleware.RequestMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
