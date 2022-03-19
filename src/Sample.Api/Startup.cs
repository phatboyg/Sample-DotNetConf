namespace Sample.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Components;
    using Components.StateMachines;
    using MassTransit;
    using MassTransit.Serialization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;


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
            services.AddMassTransit(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<OrderDbContext>();
                        r.UsePostgres();
                    });

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(Configuration.GetConnectionString("AzureServiceBus"));
                    cfg.AutoStart = true;

                    cfg.ConfigureEndpoints(context);
                });
            });
            services.Configure<MassTransitHostOptions>(options => options.WaitUntilStarted = true);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sample.Api",
                    Version = "v1"
                });
            });

            services.AddDbContext<OrderDbContext>(builder =>
            {
                builder.UseNpgsql("host=localhost;user id=postgres;password=secret;database=DotNetConfSample;", m =>
                {
                    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    m.MigrationsHistoryTable($"__{nameof(OrderDbContext)}");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
        {
            return context.Response.WriteAsync(ToJsonString(result));
        }

        static string ToJsonString(HealthReport result)
        {
            var healthResult = new JsonObject
            {
                ["status"] = result.Status.ToString(),
                ["results"] = new JsonObject(result.Entries.Select(entry => new KeyValuePair<string, JsonNode>(entry.Key,
                    new JsonObject
                    {
                        ["status"] = entry.Value.Status.ToString(),
                        ["description"] = entry.Value.Description,
                        ["data"] = JsonSerializer.SerializeToNode(entry.Value.Data, SystemTextJsonMessageSerializer.Options)
                    })))
            };

            var options = new JsonSerializerOptions(SystemTextJsonMessageSerializer.Options)
            {
                WriteIndented = true,
            };

            return healthResult.ToJsonString(options);
        }
    }
}