using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RMS.Client.Services;

public sealed class ApiClientException(
    string message,
    HttpStatusCode statusCode,
    IReadOnlyDictionary<string, string[]>? validationErrors = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; } =
        validationErrors ?? new Dictionary<string, string[]>();

    public static async Task<ApiClientException> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken);
            var message = problem?.Detail ?? problem?.Title ?? DefaultMessage(response.StatusCode);
            return new ApiClientException(message, response.StatusCode, problem?.Errors);
        }
        catch (JsonException)
        {
            return new ApiClientException(DefaultMessage(response.StatusCode), response.StatusCode);
        }
        catch (NotSupportedException)
        {
            return new ApiClientException(DefaultMessage(response.StatusCode), response.StatusCode);
        }
    }

    private static string DefaultMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "اطلاعات ارسال‌شده معتبر نیست.",
        HttpStatusCode.Unauthorized => "شماره موبایل یا رمز عبور صحیح نیست.",
        HttpStatusCode.Forbidden => "شما اجازه انجام این عملیات را ندارید.",
        HttpStatusCode.NotFound => "اطلاعات موردنظر پیدا نشد.",
        HttpStatusCode.Conflict => "این عملیات با وضعیت فعلی اطلاعات تداخل دارد.",
        HttpStatusCode.TooManyRequests => "تعداد تلاش‌ها بیش از حد مجاز است؛ کمی بعد دوباره تلاش کنید.",
        _ => "ارتباط با سرویس با خطا مواجه شد."
    };

    private sealed class ApiProblemDetails
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
