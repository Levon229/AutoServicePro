using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using AutoServicePro.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServicePro.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(
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
            logger.LogError(exception, "Request failed for {Path}", context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,
            ConflictException or DbUpdateConcurrencyException => HttpStatusCode.Conflict,
            ValidationException or ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = statusCode switch
            {
                HttpStatusCode.NotFound => "Resource not found",
                HttpStatusCode.Conflict => "The request conflicts with the current state",
                HttpStatusCode.BadRequest => "The request is invalid",
                _ => "An unexpected error occurred"
            },
            Detail = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = context.Request.Path
        };

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            context.RequestAborted);
    }
}