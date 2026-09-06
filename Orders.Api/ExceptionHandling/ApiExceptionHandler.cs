using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Orders.Domain.Exceptions;

namespace Orders.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            KeyNotFoundException => (404, "Order not found"),
            ArgumentException => (400, "Invalid request"),
            DomainRuleException => (409, "Order status conflict"),
            _ => (500, "Internal server error")
        };
        if (status == 500)
            logger.LogError(exception, "Unhandled error processing {Path}", context.Request.Path);
        else
            logger.LogWarning("Request {Path} failed: {Message}", context.Request.Path, exception.Message);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == 500 ? "An unexpected error occurred." : exception.Message,
            Instance = context.Request.Path
        }, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
