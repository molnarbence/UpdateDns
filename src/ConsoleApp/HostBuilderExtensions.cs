using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ConsoleApp;
internal static class HostBuilderExtensions
{
   internal static IHostBuilder UseSerilogWithOpenTelemetry(this IHostBuilder hostBuilder)
   {
      return hostBuilder.UseSerilog((context, services, configuration) =>
                  configuration
                     .WriteTo.Console()
                     // .WriteTo.File(context.Configuration.GetValue<string>("Serilog:LogFilePath") ?? "log.txt", rollingInterval: RollingInterval.Day)
                     .WriteTo.OpenTelemetry(
                        options =>
                        {
                           AppConfiguration appConfig = context.Configuration.Get<AppConfiguration>() ?? new AppConfiguration();
                           options.Endpoint = appConfig.OTEL_EXPORTER_OTLP_ENDPOINT;
                           var headers = appConfig.OTEL_EXPORTER_OTLP_HEADERS.Split(',') ?? [];
                           foreach (var header in headers)
                           {
                              var (key, value) = header.Split('=') switch
                              {
                              [string k, string v] => (k, v),
                                 var v => throw new Exception($"Invalid header format {v}")
                              };

                              options.Headers.Add(key, value);
                           }
                           options.ResourceAttributes.Add("service.name", "update-dns");
                        }
                     )
                  .MinimumLevel.Information()
                  .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            );
   }
}
