using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RMS.Api.ExceptionHandling;

/// <summary>خطاهای کنترل‌شدهٔ برنامه را به پاسخ استاندارد ProblemDetails تبدیل می‌کند.</summary>
public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>استثنا را به status code و بدنهٔ مناسب HTTP تبدیل می‌کند.</summary>
    /// <param name="httpContext">درخواست جاری HTTP.</param>
    /// <param name="exception">استثنای رخ‌داده.</param>
    /// <param name="cancellationToken">توکن لغو عملیات نوشتن پاسخ.</param>
    /// <returns>نتیجهٔ تلاش برای نوشتن پاسخ خطا.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException or ArgumentException =>
                (StatusCodes.Status400BadRequest, "درخواست نامعتبر است."),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "ورود یا دسترسی معتبر نیست."),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "رکورد موردنظر یافت نشد."),
            InvalidOperationException or InsufficientExecutionStackException =>
                (StatusCodes.Status409Conflict, "عملیات به دلیل وضعیت فعلی قابل انجام نیست."),
            _ =>
                (StatusCodes.Status500InternalServerError, "خطای داخلی سرور رخ داد.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled API exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
        else
            logger.LogWarning(
                "Handled API exception {ExceptionType}: {Message}. TraceId: {TraceId}",
                exception.GetType().Name,
                exception.Message,
                httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? null
                    : exception.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = httpContext.TraceIdentifier }
            },
            Exception = exception
        });
    }
}