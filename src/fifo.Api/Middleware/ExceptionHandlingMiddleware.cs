using System.Diagnostics;
using fifo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto de negocio"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Regla de negocio no cumplida"),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Autenticación fallida"),
            DbUpdateException => (StatusCodes.Status409Conflict, "No se pudo guardar la información"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado. TraceId: {TraceId}", context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(exception, "Error controlado ({StatusCode}). TraceId: {TraceId}", statusCode, context.TraceIdentifier);
        }

        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado. Contacte al administrador."
                : exception.Message,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
