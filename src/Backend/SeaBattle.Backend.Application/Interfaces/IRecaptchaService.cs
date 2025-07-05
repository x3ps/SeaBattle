namespace SeaBattle.Backend.Application.Interfaces;

/// <summary>
/// Представляет результат валидации reCAPTCHA.
/// </summary>
public class RecaptchaValidationResult
{
    /// <summary>
    /// Указывает, пройдена ли проверка reCAPTCHA успешно.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Оценка reCAPTCHA v3, если доступна.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Сообщение об ошибке, если проверка не удалась.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Список кодов ошибок, если проверка не удалась по причинам, возвращенным reCAPTCHA API.
    /// </summary>
    public IEnumerable<string>? ErrorCodes { get; set; }

    /// <summary>
    /// Статический метод для создания успешного результата валидации.
    /// </summary>
    /// <param name="score">Оценка reCAPTCHA v3.</param>
    /// <returns>Экземпляр RecaptchaValidationResult, представляющий успешную валидацию.</returns>
    public static RecaptchaValidationResult Success(double score = 1.0) =>
        new RecaptchaValidationResult { IsValid = true, Score = score };

    /// <summary>
    /// Статический метод для создания неуспешного результата валидации с сообщением об ошибке.
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке.</param>
    /// <param name="score">Оценка reCAPTCHA v3, если доступна.</param>
    /// <param name="errorCodes">Список кодов ошибок от API reCAPTCHA.</param>
    /// <returns>Экземпляр RecaptchaValidationResult, представляющий неуспешную валидацию.</returns>
    public static RecaptchaValidationResult Fail(string errorMessage, double score = 0.0, IEnumerable<string>? errorCodes = null) =>
        new RecaptchaValidationResult { IsValid = false, ErrorMessage = errorMessage, Score = score, ErrorCodes = errorCodes };
}


/// <summary>
/// Определяет контракт для сервиса валидации Google reCAPTCHA.
/// </summary>
public interface IRecaptchaService
{
    /// <summary>
    /// Валидирует токен reCAPTCHA v3, полученный от клиента.
    /// </summary>
    /// <param name="token">Токен reCAPTCHA v3.</param>
    /// <param name="remoteIp">IP-адрес пользователя, опционально.</param>
    /// <returns>Результат валидации <see cref="RecaptchaValidationResult"/>.</returns>
    Task<RecaptchaValidationResult> ValidateV3Async(string token, string? remoteIp = null);
}