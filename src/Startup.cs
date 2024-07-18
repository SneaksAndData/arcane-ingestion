using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arcane.Ingestion.Configurations;
using Arcane.Ingestion.Services.Base;
using Arcane.Ingestion.Services.Streams;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Snd.Sdk.ActorProviders;
using Snd.Sdk.Kubernetes.Providers;
using Snd.Sdk.Metrics.Configurations;
using Snd.Sdk.Metrics.Providers;
using Snd.Sdk.Storage.Providers;
using Snd.Sdk.Storage.Providers.Configurations;

namespace Arcane.Ingestion
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // service config injections
            services.Configure<JsonIngestionConfiguration>(this.Configuration.GetSection(nameof(JsonIngestionConfiguration)));


            services.AddLocalActorSystem();

            services.AddAzureBlob(AzureStorageConfiguration.CreateDefault());
            services.AddAzureTable<TableEntity>(AzureStorageConfiguration.CreateDefault());
            services.AddDatadogMetrics(DatadogConfiguration.Default(nameof(Arcane)));

            var env = AmazonStorageConfiguration.CreateFromEnv();
            services.AddAwsS3Writer(env);

            services.AddSingleton<IIngestionService<JsonDocument>, JsonIngestionService>();
            services.AddKubernetes();

            services.AddHealthChecks();

            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Arcane", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arcane v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
