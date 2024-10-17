using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Xml;
using Arcane.Ingestion.Extensions;
using Arcane.Ingestion.Services.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Arcane.Ingestion.Controllers
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("[controller]")]
    public class IngestionController : ControllerBase
    {
        private readonly ILogger<IngestionController> logger;
        private readonly IIngestionService<JsonDocument> jsonService;

        public IngestionController(ILogger<IngestionController> logger, IIngestionService<JsonDocument> jsonService)
        {
            this.logger = logger;
            this.jsonService = jsonService;
        }

        [HttpPost("json/{source}")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ObjectResult Ingest([FromBody] JsonDocument record, string source)
        {
            this.logger.LogDebug("Received JSON record for {source}", source);

            this.jsonService.Ingest(source, record);
            return this.Accepted();
        }
        
        [HttpPost("xml/{source}")]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ObjectResult Ingest([FromBody] XmlDocument record, string source)
        {
            this.logger.LogDebug("Received XML record for {source}", source);

            this.jsonService.Ingest(source, record.WrapXml());
            return this.Accepted();
        }        
    }
}
