using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace App4.RabbitConsumer.HostedService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddStackExchangeRedisCache(options =>
                    {
                        var connString =
                            $"{hostContext.Configuration["Redis:Host"]}:{hostContext.Configuration["Redis:Port"]}";
                        options.Configuration = connString;
                    });

                    services.AddOpenTelemetryTracing(builder =>
                    {
                        var provider = services.BuildServiceProvider();
                        IConfiguration config = provider
                                .GetRequiredService<IConfiguration>();

                        builder.AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .Configure((sp, builder) =>
                              {
                                  RedisCache cache = (RedisCache)sp.GetRequiredService<IDistributedCache>();
                                  builder.AddRedisInstrumentation(cache.GetConnection(), opts =>
                                  {
                                      opts.Enrich = (activity, profile) =>
                                      {
                                          activity?.SetTag("cacheCustomTag", "Enriched cache");
                                          //activity.DisplayName = profile.
                                      };
                                  });
                              })
                            .AddSource(nameof(Worker))
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App4"))
                            .AddOtlpExporter(opts =>
                            {
                                opts.Endpoint = new Uri(config["Otlp:Endpoint"]);
                                opts.ExportProcessorType = ExportProcessorType.Simple;
                            });
                    });

                });
    }
}
