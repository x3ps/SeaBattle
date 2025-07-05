using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeaBattle.Backend.Domain.Helpers;
using SeaBattle.Backend.Domain.Repositories;
using SeaBattle.Backend.Infrastructure.Data;
using SeaBattle.Backend.Infrastructure.Helpers;
using SeaBattle.Backend.Infrastructure.Repositories;

namespace SeaBattle.Backend.Infrastructure.DependencyInjection;

/// <summary>
/// Предоставляет методы расширения для <see cref="IServiceCollection"/>
/// для удобной регистрации инфраструктурных сервисов в контейнере внедрения зависимостей (DI).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет и конфигурирует сервисы слоя инфраструктуры (такие как DbContext и Unit of Work)
    /// в контейнере внедрения зависимостей.
    /// </summary>
    /// <param name="services">Коллекция сервисов для регистрации.</param>
    /// <param name="configuration">Конфигурация приложения для получения строк подключения и других настроек.</param>
    /// <returns>Обновленная коллекция сервисов.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContextPool<SeaBattleDbContext>(options => // Используем пул контекстов
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
        
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure( // Стратегия повтора при сбоях
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.MigrationsAssembly(
                    typeof(SeaBattleDbContext).Assembly.FullName);
            });
        
            options.UseQueryTrackingBehavior( 
                QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        
        return services;
    }
}