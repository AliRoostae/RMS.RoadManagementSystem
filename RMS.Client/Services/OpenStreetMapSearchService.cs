using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace RMS.Client.Services;

public sealed record OsmGeoPoint(double Latitude, double Longitude);

public sealed record OsmRoadSearchResult(
    string DisplayName,
    string Category,
    string FeatureType,
    string OsmType,
    long OsmId,
    string GeometryType,
    IReadOnlyList<IReadOnlyList<OsmGeoPoint>> Paths)
{
    public string SuggestedName => DisplayName.Split(',', StringSplitOptions.TrimEntries)[0];
}

public sealed class OpenStreetMapSearchService(
    HttpClient httpClient,
    IConfiguration configuration)
{
    private static readonly SemaphoreSlim RequestLock = new(1, 1);
    private static readonly Dictionary<string, IReadOnlyList<OsmRoadSearchResult>> Cache =
        new(StringComparer.OrdinalIgnoreCase);
    private static DateTimeOffset lastRequestUtc = DateTimeOffset.MinValue;

    private readonly string baseUrl =
        (configuration["OpenStreetMap:NominatimBaseUrl"] ?? "https://nominatim.openstreetmap.org/").TrimEnd('/') + "/";
    private readonly string countryCode = configuration["OpenStreetMap:CountryCode"] ?? "ir";
    private readonly int resultLimit = Math.Clamp(configuration.GetValue("OpenStreetMap:SearchLimit", 20), 1, 40);

    public async Task<IReadOnlyList<OsmRoadSearchResult>> SearchRoadsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        query = query.Trim();
        if (query.Length < 3)
            throw new ArgumentException("برای جست‌وجوی مسیر حداقل سه کاراکتر وارد کنید.", nameof(query));

        var cacheKey = $"{countryCode}:{query}";
        if (Cache.TryGetValue(cacheKey, out var cached))
            return cached;

        await RequestLock.WaitAsync(cancellationToken);
        try
        {
            if (Cache.TryGetValue(cacheKey, out cached))
                return cached;

            var elapsed = DateTimeOffset.UtcNow - lastRequestUtc;
            var minimumInterval = TimeSpan.FromMilliseconds(1100);
            if (elapsed < minimumInterval)
                await Task.Delay(minimumInterval - elapsed, cancellationToken);

            var uri = $"{baseUrl}search?format=geojson&polygon_geojson=1&polygon_threshold=0.00005" +
                      $"&addressdetails=1&dedupe=1&limit={resultLimit}" +
                      $"&countrycodes={Uri.EscapeDataString(countryCode)}&accept-language=fa" +
                      $"&q={Uri.EscapeDataString(query)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.ParseAdd("application/geo+json, application/json");
            using var response = await httpClient.SendAsync(request, cancellationToken);
            lastRequestUtc = DateTimeOffset.UtcNow;
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"سرویس جست‌وجوی OpenStreetMap با وضعیت {(int)response.StatusCode} پاسخ داد.");

            using var document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken)
                ?? throw new HttpRequestException("پاسخ سرویس جست‌وجوی OpenStreetMap معتبر نیست.");
            var results = ParseResults(document.RootElement);
            if (Cache.Count >= 100)
                Cache.Clear();
            Cache[cacheKey] = results;
            return results;
        }
        finally
        {
            RequestLock.Release();
        }
    }

    private static IReadOnlyList<OsmRoadSearchResult> ParseResults(JsonElement root)
    {
        if (!root.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array)
            return [];

        var results = new List<OsmRoadSearchResult>();
        foreach (var feature in features.EnumerateArray())
        {
            if (!feature.TryGetProperty("geometry", out var geometry) ||
                !geometry.TryGetProperty("type", out var geometryTypeElement) ||
                !geometry.TryGetProperty("coordinates", out var coordinates))
                continue;

            var geometryType = geometryTypeElement.GetString() ?? string.Empty;
            var paths = ParsePaths(geometryType, coordinates);
            if (paths.Count == 0 || paths.All(path => path.Count < 2))
                continue;

            var properties = feature.GetProperty("properties");
            results.Add(new OsmRoadSearchResult(
                GetString(properties, "display_name", "نام ثبت‌نشده"),
                GetString(properties, "category", string.Empty),
                GetString(properties, "type", string.Empty),
                GetString(properties, "osm_type", string.Empty),
                GetInt64(properties, "osm_id"),
                geometryType,
                paths));
        }

        return results
            .OrderByDescending(result => result.Category.Equals("highway", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(result => result.Paths.Sum(path => path.Count))
            .ToArray();
    }

    private static IReadOnlyList<IReadOnlyList<OsmGeoPoint>> ParsePaths(string geometryType, JsonElement coordinates) =>
        geometryType switch
        {
            "LineString" => [ParsePath(coordinates)],
            "MultiLineString" => coordinates.EnumerateArray().Select(ParsePath).ToArray(),
            "Polygon" => coordinates.EnumerateArray().Take(1).Select(ParsePath).ToArray(),
            "MultiPolygon" => coordinates.EnumerateArray()
                .Select(polygon => polygon.EnumerateArray().FirstOrDefault())
                .Where(ring => ring.ValueKind == JsonValueKind.Array)
                .Select(ParsePath)
                .ToArray(),
            _ => []
        };

    private static IReadOnlyList<OsmGeoPoint> ParsePath(JsonElement coordinates) => coordinates
        .EnumerateArray()
        .Where(point => point.ValueKind == JsonValueKind.Array && point.GetArrayLength() >= 2)
        .Select(point => new OsmGeoPoint(point[1].GetDouble(), point[0].GetDouble()))
        .ToArray();

    private static string GetString(JsonElement element, string propertyName, string fallback) =>
        element.TryGetProperty(propertyName, out var property) ? property.ToString() : fallback;

    private static long GetInt64(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) &&
        long.TryParse(property.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
}

public static class RoadPathBuilder
{
    private const int MaximumPointCount = 2500;

    public static IReadOnlyList<IReadOnlyList<OsmGeoPoint>> CreatePaths(
        IReadOnlyCollection<OsmRoadSearchResult> results)
    {
        if (results.Count == 0)
            throw new ArgumentException("حداقل یک قطعه مسیر لازم است.", nameof(results));

        var paths = results
            .SelectMany(result => result.Paths)
            .Select(RemoveConsecutiveDuplicates)
            .Where(path => path.Count >= 2)
            .ToArray();

        if (paths.Length == 0)
            throw new InvalidOperationException("خط معتبری برای مسیر اصلی یا شاخه‌های آن دریافت نشد.");

        var tolerance = 0.000005d;
        while (paths.Sum(path => path.Count) > MaximumPointCount && tolerance <= 0.01d)
        {
            paths = paths.Select(path => Simplify(path, tolerance)).ToArray();
            tolerance *= 1.8d;
        }

        return paths;
    }

    private static IReadOnlyList<OsmGeoPoint> RemoveConsecutiveDuplicates(
        IReadOnlyList<OsmGeoPoint> path)
    {
        var result = new List<OsmGeoPoint>(path.Count);
        foreach (var point in path)
        {
            if (result.Count == 0 || result[^1] != point)
                result.Add(point);
        }
        return result;
    }

    private static IReadOnlyList<OsmGeoPoint> Simplify(
        IReadOnlyList<OsmGeoPoint> path,
        double tolerance)
    {
        var factory = new GeometryFactory(new PrecisionModel(), 4326);
        var line = factory.CreateLineString(path
            .Select(point => new Coordinate(point.Longitude, point.Latitude))
            .ToArray());
        var simplified = (LineString)DouglasPeuckerSimplifier.Simplify(line, tolerance);
        return simplified.Coordinates
            .Select(coordinate => new OsmGeoPoint(coordinate.Y, coordinate.X))
            .ToArray();
    }
}
