using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeaBattle.Backend.Application.Interfaces;
using SeaBattle.Backend.Application.Services;
using SeaBattle.Backend.Domain.Configuration;
using SeaBattle.Backend.Domain.Helpers; 

namespace SeaBattle.Backend.Application.DependencyInjection;

/// <summary>
/// Расширения для IServiceCollection для регистрации сервисов слоя Application.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет все необходимые сервисы из слоя Application в коллекцию сервисов.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения для чтения настроек.</param>
    /// <returns>Расширенная коллекция сервисов.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Jwt));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}