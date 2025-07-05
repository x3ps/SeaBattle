using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeaBattle.Backend.Application.Interfaces;
using SeaBattle.Backend.Domain.Configuration;
using SeaBattle.Backend.Infrastructure.Services;


namespace SeaBattle.Backend.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Методы расширения для регистрации сервисов reCAPTCHA в коллекции IServiceCollection.
    /// </summary>
    public static class RecaptchaServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет сервисы Google reCAPTCHA в коллекцию IServiceCollection.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Обновленная коллекция сервисов.</returns>
        public static IServiceCollection AddRecaptchaServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RecaptchaSettings>()
                    .Bind(configuration.GetSection(RecaptchaSettings.Recaptcha))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddHttpClient("RecaptchaClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddScoped<IRecaptchaService, RecaptchaService>();

            return services;
        }
    }
}