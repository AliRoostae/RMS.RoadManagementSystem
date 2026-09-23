
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Interface;
using RMS.Application.Service;
using RMS.Domain.Interfaces;
using RMS.Domain.Rules;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Infrastructure.Persistence.EF.Repository;
using RMS.Infrastructure.Security;

namespace RMS.Api.Api.Configuration;

/// <summary>
/// ثبت وابستگی‌های لایهٔ کاربرد در ظرف تزریق وابستگی را فراهم می‌کند.
/// </summary>
public static class ApplicationConfiguration
{
    /// <summary>سرویس‌های کاربردی و سرویس ذخیره‌سازی تصویر را با طول‌عمر Scoped ثبت می‌کند.</summary>
    /// <param name="builder">سازندهٔ برنامهٔ وب.</param>
    public static void ConfigureApplication(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection تنظیم نشده است.");

        builder.Services.AddDbContext<RmsDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.UseNetTopologySuite()));

        builder.Services.AddScoped<IAccidentRepository, AccidentRepository>();
        builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        builder.Services.AddScoped<ICarRepository, CarRepository>();
        builder.Services.AddScoped<IImageRepository, ImageRepository>();
        builder.Services.AddScoped<ILookupRepository, LookupRepository>();
        builder.Services.AddScoped<INationalCode, NationalCodeRepository>();
        builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
        builder.Services.AddScoped<IPeopleRepository, PeopleRepository>();
        builder.Services.AddScoped<IRoadRepository, RoadRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        builder.Services.AddScoped<IUserAccessRules, UserAccessRules>();

        builder.Services.AddScoped<IAccident, AccidentService>();
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        builder.Services.AddScoped<ILookupService, LookupService>();
        builder.Services.AddScoped<IPassenger, PassengerService>();
        builder.Services.AddScoped<IPeople, PeopleService>();
        builder.Services.AddScoped<IRoad, RoadService>();
        builder.Services.AddScoped<IUser, UserService>();

        RegisterInternalApplicationService<ICar>(builder.Services);

        var webRootPath = builder.Environment.WebRootPath
            ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
        builder.Services.AddScoped<IImageStorageService>(provider =>
            new ImageStorageService(
                webRootPath,
                provider.GetRequiredService<IImageRepository>()));
    }

    private static void RegisterInternalApplicationService<TContract>(IServiceCollection services)
        where TContract : class
    {
        var contractType = typeof(TContract);
        var implementationType = typeof(IAccident).Assembly
            .GetTypes()
            .Single(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                contractType.IsAssignableFrom(type));

        var constructor = implementationType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderByDescending(item => item.GetParameters().Length)
            .First();

        services.AddScoped(contractType, provider =>
        {
            var arguments = constructor
                .GetParameters()
                .Select(parameter => provider.GetRequiredService(parameter.ParameterType))
                .ToArray();

            return constructor.Invoke(arguments);
        });
    }
}