using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using SmartHire.Shared;

namespace JobService
{
    public class GetJobByIdFunction
    {
        private static readonly string TableName = Environment.GetEnvironmentVariable("JOBS_TABLE_NAME")!;
        private static readonly AmazonDynamoDBClient _client = new();
#pragma warning disable CS0618
        private static readonly Table _table = Table.LoadTable(_client, TableName);
#pragma warning restore CS0618

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var allowedOrigin = Environment.GetEnvironmentVariable("ALLOWED_ORIGIN") ?? "*";

            if (request.HttpMethod == "OPTIONS")
            {
                return ApiResponseHelper.CreateCorsPreflightResponse(allowedOrigin);
            }

            if (request.PathParameters == null || !request.PathParameters.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Missing or invalid job id.",
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", allowedOrigin }
                    }
                };
            }

            try
            {
                var document = await _table.GetItemAsync(id);
                if (document == null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Body = "Job not found.",
                        Headers = new Dictionary<string, string>
                        {
                            { "Access-Control-Allow-Origin", allowedOrigin }
                        }
                    };
                }

                var job = JsonSerializer.Deserialize<Job>(document.ToJson());

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonSerializer.Serialize(job),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" },
                        { "Access-Control-Allow-Origin", allowedOrigin }
                    }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error getting job by id: {ex.Message}");

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = "Internal server error.",
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", allowedOrigin }
                    }
                };
            }
        }
    }
}
