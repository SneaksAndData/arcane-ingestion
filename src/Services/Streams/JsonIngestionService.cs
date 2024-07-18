using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Arcane.Framework.Sinks.Json;
using Arcane.Ingestion.Configurations;
using Arcane.Ingestion.Metrics;
using Arcane.Ingestion.Services.Base;
using Microsoft.Extensions.Options;
using Snd.Sdk.Metrics.Base;
using Snd.Sdk.Storage.Base;

namespace Arcane.Ingestion.Services.Streams
{
    public class JsonIngestionService : IIngestionService<JsonDocument>
    {
        private readonly IBlobStorageService blobStorageService;
        private readonly IMaterializer materializer;
        private readonly JsonIngestionConfiguration serviceConfig;
        private readonly MetricsService metricsService;
        private readonly IRunnableGraph<Sink<(string, DateTimeOffset, JsonDocument), NotUsed>> graph;
        private readonly Sink<(string, DateTimeOffset, JsonDocument), NotUsed> graphSink;

        public JsonIngestionService(IOptions<JsonIngestionConfiguration> options, IBlobStorageService blobStorageService, MetricsService metricsService, IMaterializer materializer)
        {
            this.blobStorageService = blobStorageService;
            this.materializer = materializer;
            this.serviceConfig = options.Value;
            this.metricsService = metricsService;

            this.graph = this.GetGraph();
            this.graphSink = this.graph.Run(this.materializer);
        }

        public void Ingest(string destinationName, JsonDocument json)
        {
            this.metricsService.Increment(DeclaredMetrics.DOCUMENTS_INGESTED, new SortedDictionary<string, string> { { "ingestion_source", destinationName } });
            Source.Single((destinationName, DateTimeOffset.UtcNow, json)).RunWith(this.graphSink, this.materializer);
        }

        private IRunnableGraph<Sink<(string, DateTimeOffset, JsonDocument), NotUsed>> GetGraph()
        {
            return MergeHub
                .Source<(string, DateTimeOffset, JsonDocument)>(perProducerBufferSize: this.serviceConfig.BufferSize)
                .Throttle(elements: this.serviceConfig.ThrottleDocumentLimit,
                          per: this.serviceConfig.ThrottleTimespan,
                          maximumBurst: this.serviceConfig.ThrottleDocumentBurst,
                          mode: ThrottleMode.Shaping)
                .GroupedWithin(this.serviceConfig.MaxDocumentsPerFile, this.serviceConfig.GroupingInterval)
                .SelectMany(batch => batch.GroupBy(v => v.Item1))
                .Select(v =>
                {
                    var groupName = v.Key;
                    var groupRecords = v.Select(grp => (grp.Item2, grp.Item3)).ToList();
                    this.metricsService.Gauge(DeclaredMetrics.ROWS_INGESTED, groupRecords.Count, new SortedDictionary<string, string> { { "ingestion_source", groupName } });
                    return (groupName, groupRecords);
                })
                .To(JsonSink.Create(this.blobStorageService, this.serviceConfig.IngestionSinkPath));
        }
    }
}
