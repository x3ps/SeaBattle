using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SeaBattle.Backend.Application.Interfaces;
using SeaBattle.Backend.Domain.Configuration;
using SeaBattle.Backend.Infrastructure.Models;
using System.Net.Http.Json;

namespace SeaBattle.Backend.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для взаимодействия с API Google reCAPTCHA v3.
/// </summary>
public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaSettings _recaptchaSettings;
    private readonly ILogger<RecaptchaService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RecaptchaService"/>.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика для создания экземпляров HttpClient.</param>
    /// <param name="recaptchaSettings">Опции конфигурации reCAPTCHA v3.</param>
    /// <param name="logger">Логгер для записи информации и ошибок.</param>
    public RecaptchaService(
        IHttpClientFactory httpClientFactory,
        IOptions<RecaptchaSettings> recaptchaSettings,
        ILogger<RecaptchaService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("RecaptchaClient");
        _recaptchaSettings = recaptchaSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Валидирует токен reCAPTCHA v3, полученный от клиента.
    /// </summary>
    /// <param name="token">Токен reCAPTCHA v3.</param>
    /// <param name="remoteIp">IP-адрес пользователя, опционально.</param>
    /// <returns>Результат валидации <see cref="RecaptchaValidationResult"/>.</returns>
    public async Task<RecaptchaValidationResult> ValidateV3Async(string token, string? remoteIp = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("reCAPTCHA v3 token is empty.");
            return RecaptchaValidationResult.Fail("reCAPTCHA токен не предоставлен.");
        }

        var request = new RecaptchaVerificationRequest
        {
            Secret = _recaptchaSettings.SecretKey,
            Response = token,
            RemoteIp = remoteIp
        };

        return await VerifyRecaptchaAsync(request);
    }

    /// <summary>
    /// Внутренний метод для отправки запроса на верификацию в API reCAPTCHA.
    /// </summary>
    /// <param name="request">Объект запроса к API reCAPTCHA.</param>
    /// <returns>Результат валидации <see cref="RecaptchaValidationResult"/>.</returns>
    private async Task<RecaptchaValidationResult> VerifyRecaptchaAsync(RecaptchaVerificationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_recaptchaSettings.ApiUrl, request);

            response.EnsureSuccessStatusCode();

            var recaptchaResponse = await response.Content.ReadFromJsonAsync<RecaptchaVerificationResponse>();

            if (recaptchaResponse == null)
            {
                _logger.LogError("Failed to deserialize reCAPTCHA API response.");
                return RecaptchaValidationResult.Fail("Не удалось обработать ответ reCAPTCHA.");
            }

            if (recaptchaResponse.Success)
            {
                // Проверяем оценку reCAPTCHA v3
                if (recaptchaResponse.Score < _recaptchaSettings.ScoreThreshold)
                {
                    _logger.LogWarning("reCAPTCHA v3 score {Score} is below threshold {Threshold}. User IP: {RemoteIp}",
                        recaptchaResponse.Score, _recaptchaSettings.ScoreThreshold, request.RemoteIp);
                    return RecaptchaValidationResult.Fail(
                        $"reCAPTCHA v3 оценка слишком низкая ({recaptchaResponse.Score}).",
                        recaptchaResponse.Score);
                }

                _logger.LogInformation("reCAPTCHA validation successful. Score: {Score}. User IP: {RemoteIp}",
                    recaptchaResponse.Score, request.RemoteIp);
                return RecaptchaValidationResult.Success(recaptchaResponse.Score);
            }
            else
            {
                var errorCodes = recaptchaResponse.ErrorCodes != null ? string.Join(", ", recaptchaResponse.ErrorCodes) : "No error codes provided.";
                _logger.LogWarning("reCAPTCHA validation failed. Error codes: {ErrorCodes}. User IP: {RemoteIp}",
                    errorCodes, request.RemoteIp);
                return RecaptchaValidationResult.Fail("reCAPTCHA валидация не пройдена.", 0.0, recaptchaResponse.ErrorCodes);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network or HTTP error during reCAPTCHA verification for IP: {RemoteIp}", request.RemoteIp);
            return RecaptchaValidationResult.Fail($"Ошибка сети или API при проверке reCAPTCHA: {ex.Message}");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during reCAPTCHA verification for IP: {RemoteIp}", request.RemoteIp);
            return RecaptchaValidationResult.Fail($"Непредвиденная ошибка при проверке reCAPTCHA: {ex.Message}");
        }
    }
}