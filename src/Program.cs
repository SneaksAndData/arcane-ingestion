using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Snd.Sdk.Logs.Providers;
using Snd.Sdk.Logs.Providers.Configurations;

namespace Arcane.Ingestion
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = DefaultLoggingProvider.CreateBootstrapLogger(nameof(Arcane));
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddSerilogLogger(nameof(Arcane), loggerConfiguration => loggerConfiguration.Default().AddDatadog())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
