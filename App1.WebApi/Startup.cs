using System;
using App1.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;


namespace App1.WebApi
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
            services.AddControllers();
            services.AddHttpClient();

            services.AddOpenTelemetryTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(nameof(PublishMessageController))
                    .AddSource(nameof(CallApiController))
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App1"))
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(Configuration["Otlp:Endpoint"]);
                        opts.ExportProcessorType = ExportProcessorType.Simple;
                    });
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddOpenTelemetryMetrics(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App1"))
                .AddMeter("App1-Metrics")
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(Configuration["Otlp:Endpoint"]);
                })
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync("Ok");
                });
                endpoints.MapControllers();
            });
        }
    }
}
