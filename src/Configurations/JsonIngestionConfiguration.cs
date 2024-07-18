using System;
using System.Diagnostics.CodeAnalysis;

namespace Arcane.Ingestion.Configurations;

/// <summary>
/// Configuration for a json ingestion endpoint.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Model")]
public class JsonIngestionConfiguration
{
    /// <summary>
    /// Size of an Akka MergeHub Buffer for this endpoint.
    /// </summary>
    public int BufferSize { get; set; }

    /// <summary>
    /// Document processing rate per <see cref="ThrottleTimespan"/>.
    /// </summary>
    public int ThrottleDocumentLimit { get; set; }

    /// <summary>
    /// Number of documents to receive before throttling kicks in.
    /// </summary>
    public int ThrottleDocumentBurst { get; set; }

    /// <summary>
    /// Document processing rate (time).
    /// </summary>
    public TimeSpan ThrottleTimespan { get; set; }

    /// <summary>
    /// Max number of JSON documents in a single output file.
    /// </summary>
    public int MaxDocumentsPerFile { get; set; }

    /// <summary>
    /// Grouping interval for received records.
    /// </summary>
    public TimeSpan GroupingInterval { get; set; }

    /// <summary>
    /// Base location to save data in. Must follow format required by underlying storage service (az, s3 etc.).
    /// </summary>
    public string IngestionSinkPath { get; set; }
}
