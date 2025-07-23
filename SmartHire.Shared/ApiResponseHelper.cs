using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;


namespace SmartHire.Shared
{
    public static class ApiResponseHelper
    {
        public static APIGatewayProxyResponse CreateCorsPreflightResponse(string allowedOrigin)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = GetCorsHeaders(allowedOrigin)
            };
        }

        public static Dictionary<string, string> GetCorsHeaders(string allowedOrigin)
        {
            return new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", allowedOrigin },
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "GET,POST,OPTIONS" }
            };
        }
    }
}
