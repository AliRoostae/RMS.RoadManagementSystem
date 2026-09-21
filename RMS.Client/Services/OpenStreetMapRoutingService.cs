using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text.Json;

namespace RMS.Client.Services;

public sealed record OsmRouteResult(
    IReadOnlyList<OsmGeoPoint> Points,
    double DistanceMeters,
    double DurationSeconds);

public sealed class OpenStreetMapRoutingService(
    HttpClient httpClient,
    IConfiguration configuration)
{
    private readonly string baseUrl =
        (configuration["OpenStreetMap:RoutingBaseUrl"] ?? "https://router.project-osrm.org/")
        .TrimEnd('/');

    private readonly string profile =
        configuration["OpenStreetMap:RoutingProfile"] ?? "driving";

    public async Task<OsmRouteResult> FindRouteAsync(
        OsmGeoPoint start,
        OsmGeoPoint end,
        CancellationToken cancellationToken = default)
    {
        ValidatePoint(start, nameof(start));
        ValidatePoint(end, nameof(end));

        var coordinates = $"{Format(start.Longitude)},{Format(start.Latitude)};" +
                          $"{Format(end.Longitude)},{Format(end.Latitude)}";
        var requestUri = $"{baseUrl}/route/v1/{Uri.EscapeDataString(profile)}/{coordinates}" +
                         "?overview=full&geometries=geojson&steps=false&alternatives=false";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"سرویس مسیریابی با کد {(int)response.StatusCode} پاسخ داد.",
                null,
                response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var code = root.TryGetProperty("code", out var codeElement)
            ? codeElement.GetString()
            : null;
        if (!string.Equals(code, "Ok", StringComparison.OrdinalIgnoreCase))
        {
            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : null;
            throw new InvalidOperationException(message ?? "بین مبدأ و مقصد انتخاب‌شده مسیر قابل عبوری پیدا نشد.");
        }

        if (!root.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
            throw new InvalidOperationException("بین مبدأ و مقصد انتخاب‌شده مسیری پیدا نشد.");

        var route = routes[0];
        if (!route.TryGetProperty("geometry", out var geometry) ||
            !geometry.TryGetProperty("coordinates", out var routeCoordinates))
            throw new InvalidOperationException("هندسه مسیر از سرویس مسیریابی دریافت نشد.");

        var points = routeCoordinates
            .EnumerateArray()
            .Where(coordinate => coordinate.GetArrayLength() >= 2)
            .Select(coordinate => new OsmGeoPoint(
                coordinate[1].GetDouble(),
                coordinate[0].GetDouble()))
            .ToArray();
        if (points.Length < 2)
            throw new InvalidOperationException("تعداد نقاط مسیر دریافت‌شده کافی نیست.");

        var distance = route.TryGetProperty("distance", out var distanceElement)
            ? distanceElement.GetDouble()
            : 0d;
        var duration = route.TryGetProperty("duration", out var durationElement)
            ? durationElement.GetDouble()
            : 0d;

        return new OsmRouteResult(points, distance, duration);
    }

    private static string Format(double value) =>
        value.ToString("0.######", CultureInfo.InvariantCulture);

    private static void ValidatePoint(OsmGeoPoint point, string parameterName)
    {
        if (point.Latitude is < -90 or > 90 || point.Longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(parameterName, "مختصات انتخاب‌شده معتبر نیست.");
    }
}
