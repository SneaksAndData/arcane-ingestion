using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Arcane.Ingestion.Extensions;

/// <summary>
/// Utility methods for pre-processing payloads for the ingestion service.
/// </summary>
public static class IngestionExtensions
{
    /// <summary>
    /// Wraps XML input as base64-encoded content data and returns a JsonDocument.
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static JsonDocument WrapXml(this XmlDocument xml)
    {
        var contentBytes = Encoding.UTF8.GetBytes(xml.OuterXml);
        return JsonSerializer.SerializeToDocument(new
        {
            ContentData = Convert.ToBase64String(contentBytes),
            ContentMd5 = Convert.ToBase64String(MD5.HashData(contentBytes))
        });
    }
}
