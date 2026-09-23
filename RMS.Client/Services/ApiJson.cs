using System.Text.Json;
using NetTopologySuite.IO.Converters;

namespace RMS.Client.Services;

internal static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new GeoJsonConverterFactory());
        return options;
    }
}