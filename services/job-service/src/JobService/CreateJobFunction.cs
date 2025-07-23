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
    public class CreateJobFunction
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

            try
            {
                var job = JsonSerializer.Deserialize<Job>(request.Body);

                if (job == null ||
                    string.IsNullOrWhiteSpace(job.id) ||
                    string.IsNullOrWhiteSpace(job.Title) ||
                    string.IsNullOrWhiteSpace(job.Company) ||
                    string.IsNullOrWhiteSpace(job.Location) ||
                    string.IsNullOrWhiteSpace(job.Category))
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Missing required fields in job payload.",
                        Headers = new Dictionary<string, string>
                        {
                            { "Access-Control-Allow-Origin", allowedOrigin }
                        }
                    };
                }

                var document = new Document
                {
                    ["id"] = job.id,
                    ["Title"] = job.Title,
                    ["Company"] = job.Company,
                    ["Location"] = job.Location,
                    ["Category"] = job.Category
                };

                await _table.PutItemAsync(document);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Body = "Job created successfully.",
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", allowedOrigin }
                    }
                };
            }
            catch (JsonException jsonEx)
            {
                context.Logger.LogLine($"Invalid JSON format: {jsonEx.Message}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Invalid JSON format.",
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", allowedOrigin }
                    }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Unexpected error: {ex}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = "An unexpected error occurred.",
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", allowedOrigin }
                    }
                };
            }
        }
    }
}
