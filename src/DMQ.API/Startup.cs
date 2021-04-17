using DMQ.MessageComponents.Consumers;
using DMQ.MessageContracts;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DMQ.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup container configuration for MT. E.g., consumers, sagas, etc.. 
            services.AddMassTransit(config => {
                config.AddConsumer<SubmitOrderConsumer>();

                // Add in-memory message transport for testing purpose using MediatR, without a message broker.
                config.AddMediator();

                config.AddRequestClient<ISubmitOrder>();

            });

            // MVC controllers
            services.AddControllers();

            // Swagger 
            services.AddOpenApiDocument(cfg => 
            {
                cfg.PostProcess = d => d.Info.Title = "Distributed Message Queue System - API";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Swagger UI
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
