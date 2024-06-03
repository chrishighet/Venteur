using System.Net;
using KnightPath.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using KnightPath.Models;
using Azure.Storage.Queues.Models;
using System.Text.Json;

namespace KnightPath
{
    public class KnightPathFunctions
    {
        private readonly ILogger _logger;
        private readonly IKnightService _knightService;
        private const string queueName = "knightpath";
        private const string knightPathRoute = "knightpath";

        public KnightPathFunctions(ILoggerFactory loggerFactory, IKnightService knightService)
        {
            _logger = loggerFactory.CreateLogger<KnightPathFunctions>();
            _knightService = knightService;
        }

        [Function("HttpCommandKnightPosition")]
        public async Task<HttpResponseData> CommandKnightPositionAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = knightPathRoute)] HttpRequestData req)
        {
            var response = req.CreateResponse();
            try
            {
                string? source = req.Query["source"];
                string? target = req.Query["target"];

                var result = await _knightService.QueueKnightMoveAsync(source, target);

                response.StatusCode = HttpStatusCode.OK;

                await response.WriteStringAsync($"Operation Id {result} was created.Please query it to find your results.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while queuing the knight's movement");

                response.StatusCode = HttpStatusCode.InternalServerError;

                await response.WriteStringAsync("An internal server error has occurred");
            }

            return response;
        }

        [Function("HttpGetKnightPosition")]
        public async Task<HttpResponseData> GetKnightPositionAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = knightPathRoute)] HttpRequestData req)
        {
            var response = req.CreateResponse();
            try
            {
                string? operationId = req.Query["operationId"];

                var result = _knightService.GetKnightPathResponse(operationId);

                if (result is null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;

                    await response.WriteStringAsync($"Operation Id {operationId} was not found.");

                    return response;
                }

                response.StatusCode = HttpStatusCode.OK;

                response.Headers.Add("Content-Type", "text/json; charset=utf-8");

                await response.WriteStringAsync(JsonSerializer.Serialize(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while getting the knight's current position");

                response.StatusCode = HttpStatusCode.InternalServerError;

                await response.WriteStringAsync("An internal server error has occurred");
            }

            return response;
        }

        [Function("EventHubsMoveKnightPosition")]
        public void MoveKnightPosition([QueueTrigger(queueName, Connection = "AzureWebJobsStorage")] QueueMessage queueMessage, ILogger log)
        {
            var request = JsonSerializer.Deserialize<Request>(queueMessage.MessageText);
            if (request is null)
            {
                _logger.LogError("Queue message was null");
                return;
            }

            _knightService.MoveKnight(request);
        }

    }
}
