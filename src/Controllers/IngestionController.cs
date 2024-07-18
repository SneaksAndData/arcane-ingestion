using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
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
            this.logger.LogDebug("Received record for {source}", source);

            this.jsonService.Ingest(source, record);
            return this.Accepted();
        }
    }
}
