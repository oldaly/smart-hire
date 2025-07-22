using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobService
{
    public class Job
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Company { get; set; } = "";
        public string Location { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class Function
    {
        private static readonly string TableName = Environment.GetEnvironmentVariable("JOBS_TABLE_NAME")!;
        private static readonly AmazonDynamoDBClient _client = new();

#pragma warning disable CS0618
        private static readonly Table _table = Table.LoadTable(_client, TableName);
#pragma warning restore CS0618

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var allowedOrigin = Environment.GetEnvironmentVariable("ALLOWED_ORIGIN") ?? "*";
            // Handle CORS preflight request
            if (request.HttpMethod == "OPTIONS")
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", allowedOrigin },
                        { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                        { "Access-Control-Allow-Methods", "GET,OPTIONS" }
                    }
                };
            }

            // Read optional "category" query param
            string? category = null;
            if (request.QueryStringParameters != null)
                request.QueryStringParameters.TryGetValue("category", out category);

            var scanFilter = new ScanFilter();
            if (!string.IsNullOrEmpty(category))
            {
                scanFilter.AddCondition("Category", ScanOperator.Equal, category);
            }

            var search = _table.Scan(scanFilter);
            var jobs = new List<Job>();

            do
            {
                var documents = await search.GetNextSetAsync();
                jobs.AddRange(documents.Select(doc => JsonSerializer.Deserialize<Job>(doc.ToJson())!));
            }
            while (!search.IsDone);

            context.Logger.LogLine($"Returning {jobs.Count} job(s)");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(jobs),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", allowedOrigin },
                    { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                    { "Access-Control-Allow-Methods", "GET,OPTIONS" }
                }
            };
        }
    }
}
