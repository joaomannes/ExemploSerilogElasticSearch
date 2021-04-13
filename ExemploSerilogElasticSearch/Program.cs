using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExemploSerilogElasticSearch
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var hostBuilder = CreateHostBuilder(args).Build();
                Serilog.Log.Information("Iniciando Web Host");
                hostBuilder.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Host encerrado inesperadamente");
                return 1;
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settings = config.Build();
                    Serilog.Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Elasticsearch(
                            options:
                                new ElasticsearchSinkOptions(
                                    new Uri(settings["Elasticsearch:Uri"]))
                                {
                                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                                       EmitEventFailureHandling.WriteToFailureSink |
                                                       EmitEventFailureHandling.RaiseCallback,
                                    FailureSink = new LoggerConfiguration().WriteTo.File(new JsonFormatter(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"erros.txt")).CreateLogger(),
                                    AutoRegisterTemplate = true,
                                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                    IndexFormat = settings["AppName"] + "-{0:yyyy.MM}"
                                })
                        .WriteTo.Console()
                        .CreateLogger();
                })
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
