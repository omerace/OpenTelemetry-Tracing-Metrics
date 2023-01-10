using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using App3.WebApi.Domain.Interfaces;
using App3.WebApi.Infrastructure.Metrics;
using App3.WebApi.Infrastructure.Repositories;
using App3.WebApi.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace App3.WebApi
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
            var meter = new Meter("App3");
            services.AddAutoMapper(typeof(Program));
            services.RegisterInfrastureDependencies(Configuration);

            services.AddTransient<ISqlRepository, SqlRepository>();
            services.AddTransient<IRabbitRepository, RabbitRepository>();
            services.AddControllers().AddNewtonsoftJson();
            //var allControllers = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(ControllerBase))).Select(x => x.Name);
            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(Configuration["Otlp:Endpoint"]);
                        opts.ExportProcessorType = ExportProcessorType.Simple;
                    })
                    .AddHttpClientInstrumentation() // Http isteklerinin konfigürasyonunu saðlar
                    .AddAspNetCoreInstrumentation(opts => // Gelen requestlerin konfigürasyonunu saðþlar
                    {
                        opts.Enrich = (activity, eventName, rawObject) =>
                        {
                            if (eventName.Equals("OnStartActivity"))
                            {
                                if (rawObject is HttpRequest httpRequest)
                                {
                                    activity.SetTag("physicalPath", httpRequest.Path);
                                    //activity.DisplayName = httpRequest.Path; // Http spanýnýn ismini deðiþtirir
                                }
                            }
                            else if (eventName.Equals("OnStopActivity"))
                            {
                                if (rawObject is HttpResponse httpResponse)
                                {
                                    activity.SetTag("responseType", httpResponse.ContentType);
                                }
                            }
                        };
                    })
                    .AddSqlClientInstrumentation(opts => //sql sorgularýnýn konfigürasyonunu saðlar
                    {
                        opts.SetDbStatementForText = true; // sql sorgularýný gösterir
                        opts.RecordException = true; // sorguda exception alýndýðýnda ayrý bir span halinde verir.
                    })
                    .AddSource(Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(ControllerBase))).Select(x => x.Name).ToArray())
                    .AddSource(nameof(RabbitRepository))
                    //.AddSource(nameof(CategoriesController))
                    //.AddSource("InventoryBackgroundJob")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(meter.Name));
            });

            services.AddOpenTelemetryMetrics(metricProviderBuilder =>
            {
                metricProviderBuilder
                     .AddOtlpExporter(opts =>
                     {
                         opts.Endpoint = new Uri(Configuration["Otlp:Endpoint"]);
                     })
                    .AddMeter(meter.Name)
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(meter.Name))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

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
