using InternetCafe.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InternetCafe.Common.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;

            if (exception is AuthenticationException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                message = GetErrorMessage(exception),
                errors = exception is DomainException ? new[] { exception.Message } : null
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }

        private static string GetErrorMessage(Exception exception)
        {
            if (exception is DomainException ||
                exception is AuthenticationException ||
                exception is UnauthorizedAccessException)
            {
                return exception.Message;
            }

            return "An error occurred while processing your request.";
        }
    }
}
