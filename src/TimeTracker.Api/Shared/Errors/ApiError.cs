using FluentValidation.Results;

namespace TimeTracker.Api.Shared.Errors;

public sealed record ApiError(string Code, string Message, object? Details = null);


public static class ApiErrors
{
    public static IResult ValidationError(ValidationResult result)
        => Results.BadRequest(new
        {
            code = "VALIDATION_ERROR",
            message = "Invalid request",
            errors = result.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            })
        });

    public static IResult NotFound(string code, string message)
        => Results.NotFound(new { code, message });

    public static IResult Conflict(string code, string message)
        => Results.Conflict(new { code, message });

    public static IResult Unauthorized(string code, string message)
    => Results.Json(new { code, message }, statusCode: StatusCodes.Status401Unauthorized);

    public static IResult Forbidden(string code, string message, object? details = null)
        => Results.Json(new { code, message, details }, statusCode: StatusCodes.Status403Forbidden);

    public static IResult Conflict(string code, string message, object? details = null)
        => Results.Conflict(new { code, message, details });

    public static IResult BadRequest(string code, string message, object? details = null)
        => Results.BadRequest(new { code, message, details });
}