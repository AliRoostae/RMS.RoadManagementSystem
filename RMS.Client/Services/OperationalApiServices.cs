using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Client.Services;

public sealed class AnalyticsApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<DashboardOverviewResponse> GetDashboardAsync(
        DashboardQuery query,
        CancellationToken token = default) =>
        GetAsync<DashboardOverviewResponse>(
            $"api/dashboard/overview?startTime={query.StartTime}&endTime={query.EndTime}" +
            $"&months={query.Months}&recentCount={query.RecentCount}",
            token);

    public Task<AccidentReportResponse> GetAccidentReportAsync(
        AccidentReportQuery query,
        CancellationToken token = default)
    {
        var uri = $"api/reports/accidents?startTime={query.StartTime}&endTime={query.EndTime}&months={query.Months}";
        if (query.RoadId.HasValue)
            uri += $"&roadId={query.RoadId.Value}";
        if (query.AccidentType.HasValue)
            uri += $"&accidentType={Convert.ToByte(query.AccidentType.Value)}";
        return GetAsync<AccidentReportResponse>(uri, token);
    }

    /// <summary>گزارش صفحه‌بندی‌شده افراد را با فیلترهای HumanQueries دریافت می‌کند.</summary>
    public Task<PagedResponse<HumanItemResponse>> GetHumanReportAsync(
        HumanQueries query,
        CancellationToken token = default)
    {
        var uri = $"api/reports/humans?skip={query.Skip}&take={query.Take}";
        if (!string.IsNullOrWhiteSpace(query.NationalCode))
            uri += $"&nationalCode={Uri.EscapeDataString(query.NationalCode.Trim())}";
        if (query.IsDriver.HasValue)
            uri += $"&isDriver={query.IsDriver.Value.ToString().ToLowerInvariant()}";
        if (query.DamageType.HasValue)
            uri += $"&damageType={Convert.ToByte(query.DamageType.Value)}";
        return GetAsync<PagedResponse<HumanItemResponse>>(uri, token);
    }
}

public sealed class AccidentApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<PagedResponse<AccidentListItemResponse>> GetAllAsync(AccidentQuery query, CancellationToken token = default)
    {
        var uri = $"api/accidents?startTime={query.StartTime}&endTime={query.EndTime}&skip={query.Skip}&take={query.Take}" +
            $"&orderBy={query.OrderBy}";
        if (query.RoadId.HasValue)
            uri += $"&roadId={query.RoadId.Value}";
        if (query.AccidentType.HasValue)
            uri += $"&accidentType={Convert.ToByte(query.AccidentType.Value)}";
        if (query.Weather.HasValue)
            uri += $"&weather={Convert.ToByte(query.Weather.Value)}";
        if (query.Cause.HasValue)
            uri += $"&cause={Convert.ToByte(query.Cause.Value)}";
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            uri += $"&searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}";
        return GetAsync<PagedResponse<AccidentListItemResponse>>(uri, token);
    }

    public Task<AccidentResponse> GetByIdAsync(Guid id, CancellationToken token = default) =>
        GetAsync<AccidentResponse>($"api/accidents/{id}", token);

    public Task<IReadOnlyList<AccidentMapPointResponse>> GetMapAsync(
        AccidentMapQuery query,
        CancellationToken token = default)
    {
        var uri = $"api/accidents/map?startTime={query.StartTime}&endTime={query.EndTime}&take={query.Take}";
        if (query.RoadId.HasValue)
            uri += $"&roadId={query.RoadId.Value}";
        if (query.MinLatitude.HasValue)
        {
            uri += $"&minLatitude={Invariant(query.MinLatitude.Value)}&maxLatitude={Invariant(query.MaxLatitude!.Value)}" +
                $"&minLongitude={Invariant(query.MinLongitude!.Value)}&maxLongitude={Invariant(query.MaxLongitude!.Value)}";
        }
        return GetAsync<IReadOnlyList<AccidentMapPointResponse>>(uri, token);
    }

    public Task CreateAsync(CreateAccidentCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Post, "api/accidents", command, token);

    public Task UpdateAsync(Guid id, UpdateAccidentCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Put, $"api/accidents/{id}", command, token);

    public Task DeleteAsync(Guid id, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/accidents/{id}", cancellationToken: token);

    private static string Invariant(double value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

public sealed class CarApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<PagedResponse<CarListItemResponse>> GetAllAsync(CarQueries query, CancellationToken token = default)
    {
        var plate = string.IsNullOrWhiteSpace(query.Plate)
            ? string.Empty
            : $"&plate={Uri.EscapeDataString(query.Plate.Trim())}";
        var search = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? string.Empty
            : $"&searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}";
        var accident = query.AccidentId.HasValue ? $"&accidentId={query.AccidentId.Value}" : string.Empty;
        var carClass = query.CarClass.HasValue ? $"&carClass={Convert.ToByte(query.CarClass.Value)}" : string.Empty;
        return GetAsync<PagedResponse<CarListItemResponse>>(
            $"api/cars?skip={query.Skip}&take={query.Take}{plate}{search}{accident}{carClass}", token);
    }

    public Task<CarResponse> GetByIdAsync(Guid id, CancellationToken token = default) =>
        GetAsync<CarResponse>($"api/cars/{id}", token);

    public Task CreateAsync(CreateCarCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Post, "api/cars", command, token);

    public Task UpdateAsync(Guid id, UpdateCarCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Put, $"api/cars/{id}", command, token);

    public Task DeleteAsync(Guid id, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/cars/{id}", cancellationToken: token);
}

public sealed class PassengerApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<PagedResponse<PassengerListItemResponse>> GetAllAsync(PassengerQueries query, CancellationToken token = default) =>
        GetAsync<PagedResponse<PassengerListItemResponse>>(BuildUri("api/passengers", query), token);

    public Task<PassengerResponse> GetByIdAsync(Guid id, CancellationToken token = default) =>
        GetAsync<PassengerResponse>($"api/passengers/{id}", token);

    public Task CreateAsync(CreatePassengerCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Post, "api/passengers", command, token);

    public Task UpdateAsync(Guid id, UpdatePassengerCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Put, $"api/passengers/{id}", command, token);

    public Task DeleteAsync(Guid id, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/passengers/{id}", cancellationToken: token);

    private static string BuildUri(string path, PassengerQueries query)
    {
        var nationalCode = string.IsNullOrWhiteSpace(query.NationalCode)
            ? string.Empty
            : $"&nationalCode={Uri.EscapeDataString(query.NationalCode.Trim())}";
        var search = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? string.Empty
            : $"&searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}";
        var car = query.CarId.HasValue ? $"&carId={query.CarId.Value}" : string.Empty;
        var accident = query.AccidentId.HasValue ? $"&accidentId={query.AccidentId.Value}" : string.Empty;
        var driver = query.IsDriver.HasValue ? $"&isDriver={query.IsDriver.Value.ToString().ToLowerInvariant()}" : string.Empty;
        var damage = query.DamageType.HasValue ? $"&damageType={Convert.ToByte(query.DamageType.Value)}" : string.Empty;
        return $"{path}?skip={query.Skip}&take={query.Take}{nationalCode}{search}{car}{accident}{driver}{damage}";
    }
}

public sealed class PeopleApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<PagedResponse<PeopleListItemResponse>> GetAllAsync(PeopleQueries query, CancellationToken token = default) =>
        GetAsync<PagedResponse<PeopleListItemResponse>>(BuildUri("api/people", query), token);

    public Task<PeopleResponse> GetByIdAsync(Guid id, CancellationToken token = default) =>
        GetAsync<PeopleResponse>($"api/people/{id}", token);

    public Task CreateAsync(CreatePeopleCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Post, "api/people", command, token);

    public Task UpdateAsync(Guid id, UpdatePeopleCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Put, $"api/people/{id}", command, token);

    public Task DeleteAsync(Guid id, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/people/{id}", cancellationToken: token);

    private static string BuildUri(string path, PeopleQueries query)
    {
        var nationalCode = string.IsNullOrWhiteSpace(query.NationalCode)
            ? string.Empty
            : $"&nationalCode={Uri.EscapeDataString(query.NationalCode.Trim())}";
        var search = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? string.Empty
            : $"&searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}";
        var accident = query.AccidentId.HasValue ? $"&accidentId={query.AccidentId.Value}" : string.Empty;
        var damage = query.DamageType.HasValue ? $"&damageType={Convert.ToByte(query.DamageType.Value)}" : string.Empty;
        return $"{path}?skip={query.Skip}&take={query.Take}{nationalCode}{search}{accident}{damage}";
    }
}

public sealed class RoadApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<PagedResponse<RoadListItemResponse>> GetAllAsync(RoadQueries query, CancellationToken token = default)
    {
        var search = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? string.Empty
            : $"&searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}";
        return GetAsync<PagedResponse<RoadListItemResponse>>(
            $"api/roads?skip={query.Skip}&take={query.Take}{search}", token);
    }

    public Task<IReadOnlyList<RoadMapItemResponse>> GetMapAsync(
        RoadMapQuery query,
        CancellationToken token = default)
    {
        var uri = $"api/roads/map?zoom={query.Zoom}&take={query.Take}";
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            uri += $"&searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}";
        if (query.MinLatitude.HasValue)
        {
            uri += $"&minLatitude={Invariant(query.MinLatitude.Value)}&maxLatitude={Invariant(query.MaxLatitude!.Value)}" +
                $"&minLongitude={Invariant(query.MinLongitude!.Value)}&maxLongitude={Invariant(query.MaxLongitude!.Value)}";
        }
        return GetAsync<IReadOnlyList<RoadMapItemResponse>>(uri, token);
    }

    public Task<RoadResponse> GetByIdAsync(Guid id, CancellationToken token = default) =>
        GetAsync<RoadResponse>($"api/roads/{id}", token);

    public Task<RoadGeometryResponse> GetGeometryAsync(Guid id, CancellationToken token = default) =>
        GetAsync<RoadGeometryResponse>($"api/roads/{id}/geometry", token);

    public Task<RoadLocationResponse> GetNearestPointAsync(
        Guid id,
        double latitude,
        double longitude,
        double maxDistanceMeters = 100,
        CancellationToken token = default) =>
        GetAsync<RoadLocationResponse>(
            $"api/roads/{id}/nearest-point?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&maxDistanceMeters={maxDistanceMeters.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            token);

    public Task CreateAsync(CreateRoadCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Post, "api/roads", command, token);

    public Task UpdateAsync(Guid id, UpdateRoadCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Put, $"api/roads/{id}", command, token);

    public Task DeleteAsync(Guid id, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/roads/{id}", cancellationToken: token);

    private static string Invariant(double value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

public sealed class LookupApiService(AuthenticationService authenticationService)
    : AuthorizedApiService(authenticationService)
{
    public Task<IReadOnlyList<RoadLookupResponse>> GetRoadsAsync(
        LookupQuery query,
        CancellationToken token = default) =>
        GetAsync<IReadOnlyList<RoadLookupResponse>>(
            BuildUri("api/lookups/roads", query.SearchTerm, query.Take), token);

    public Task<IReadOnlyList<AccidentLookupResponse>> GetAccidentsAsync(
        AccidentLookupQuery query,
        CancellationToken token = default)
    {
        var uri = BuildUri("api/lookups/accidents", query.SearchTerm, query.Take);
        if (query.RoadId.HasValue)
            uri += $"&roadId={query.RoadId.Value}";
        return GetAsync<IReadOnlyList<AccidentLookupResponse>>(uri, token);
    }

    public Task<IReadOnlyList<CarLookupResponse>> GetCarsAsync(
        CarLookupQuery query,
        CancellationToken token = default)
    {
        var uri = BuildUri("api/lookups/cars", query.SearchTerm, query.Take);
        if (query.AccidentId.HasValue)
            uri += $"&accidentId={query.AccidentId.Value}";
        return GetAsync<IReadOnlyList<CarLookupResponse>>(uri, token);
    }

    private static string BuildUri(string path, string? searchTerm, int take)
    {
        var search = string.IsNullOrWhiteSpace(searchTerm)
            ? string.Empty
            : $"&searchTerm={Uri.EscapeDataString(searchTerm.Trim())}";
        return $"{path}?take={take}{search}";
    }
}

public sealed class ImageApiService(
    AuthenticationService authenticationService,
    HttpClient httpClient) : AuthorizedApiService(authenticationService)
{
    public Task<IReadOnlyList<ImageResponse>> GetAllAsync(Guid accidentId, CancellationToken token = default) =>
        GetAsync<IReadOnlyList<ImageResponse>>($"api/images/accident/{accidentId}", token);

    public Task<ImageResponse> GetByIdAsync(Guid id, CancellationToken token = default) =>
        GetAsync<ImageResponse>($"api/images/{id}", token);

    public Task CreateAsync(CreateAccidentImageCommand command, CancellationToken token = default) =>
        SendAsync(HttpMethod.Post, "api/images", command, token);

    public Task DeleteAsync(Guid id, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/images/{id}", cancellationToken: token);

    public Task DeleteAllAsync(Guid accidentId, CancellationToken token = default) =>
        SendAsync(HttpMethod.Delete, $"api/images/accident/{accidentId}", cancellationToken: token);

    public string ResolveUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            imageUrl.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }


        var baseAddress = httpClient.BaseAddress
            ?? throw new InvalidOperationException("نشانی پایهٔ API تنظیم نشده است.");
        var resDef = new Uri(baseAddress, imageUrl.TrimStart('/')).ToString();
        return resDef;
    }
}
