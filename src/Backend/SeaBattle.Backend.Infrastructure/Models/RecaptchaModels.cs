using System.Text.Json.Serialization;

namespace SeaBattle.Backend.Infrastructure.Models
{
    /// <summary>
    /// Модель данных для запроса к API Google reCAPTCHA.
    /// </summary>
    public class RecaptchaVerificationRequest
    {
        /// <summary>
        /// Секретный ключ reCAPTCHA.
        /// </summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// Токен ответа reCAPTCHA, полученный на клиентской стороне.
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// IP-адрес конечного пользователя. Опционально.
        /// </summary>
        [JsonPropertyName("remoteip")]
        public string? RemoteIp { get; set; }
    }

    /// <summary>
    /// Модель данных для ответа от API Google reCAPTCHA.
    /// </summary>
    public class RecaptchaVerificationResponse
    {
        /// <summary>
        /// Флаг успеха проверки reCAPTCHA.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Оценка риска от reCAPTCHA v3 (от 0.0 до 1.0), где 1.0 — это очень вероятно, что это хороший пользователь, 0.0 — это очень вероятно, что это бот.
        /// Присутствует только для reCAPTCHA v3.
        /// </summary>
        [JsonPropertyName("score")]
        public double Score { get; set; }

        /// <summary>
        /// Действие, которое было связано с этим вызовом (предоставляется клиентом).
        /// Присутствует только для reCAPTCHA v3.
        /// </summary>
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        /// <summary>
        /// Отметка времени проверки (ISO8601 формат).
        /// </summary>
        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTimestamp { get; set; }

        /// <summary>
        /// Имя хоста сайта, для которого был выдан ответ.
        /// </summary>
        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        /// <summary>
        /// Массив кодов ошибок, если проверка не удалась.
        /// </summary>
        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}