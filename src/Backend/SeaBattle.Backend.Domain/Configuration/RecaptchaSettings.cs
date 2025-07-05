using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Backend.Domain.Configuration;

/// <summary>
/// Опции конфигурации для Google reCAPTCHA v3.
/// </summary>
public class RecaptchaSettings
{
    public const string Recaptcha = "Recaptcha";

    /// <summary>
    /// Публичный ключ (Site Key) для reCAPTCHA v3.
    /// Используется на клиентской стороне для инициализации reCAPTCHA.
    /// </summary>
    [Required(ErrorMessage = "SiteKeyV3 обязателен для настройки reCAPTCHA v3.")]
    public string SiteKey { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ (Secret Key) для reCAPTCHA v3.
    /// Используется на серверной стороне для валидации ответа reCAPTCHA.
    /// </summary>
    [Required(ErrorMessage = "SecretKeyV3 обязателен для настройки reCAPTCHA v3.")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Минимальный допустимый порог оценки для reCAPTCHA v3 (от 0.0 до 1.0).
    /// Если оценка ниже этого значения, валидация считается неуспешной.
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "ScoreThresholdV3 должен быть в диапазоне от 0.0 до 1.0.")]
    public double ScoreThreshold { get; set; } = 0.7;

    /// <summary>
    /// Базовый URL для API проверки reCAPTCHA.
    /// По умолчанию используется https://www.google.com/recaptcha/api/siteverify
    /// </summary>
    [Url(ErrorMessage = "RecaptchaApiUrl должен быть действительным URL.")]
    public string ApiUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
}