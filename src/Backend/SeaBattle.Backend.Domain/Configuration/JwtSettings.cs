using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Backend.Domain.Configuration;

/// <summary>
/// Опции конфигурации для JSON Web Tokens (JWT).
/// </summary>
public class JwtSettings
{
    public const string Jwt = "Jwt";

    /// <summary>
    /// Секретный ключ для подписи JWT. Должен быть длинным и сложным.
    /// Зачем нужно: Для криптографической подписи токенов, обеспечения их целостности и подлинности.
    /// </summary>
    [Required(ErrorMessage = "Секретный ключ JWT является обязательным.")]
    public string Secret { get; set; } = null!;

    /// <summary>
    /// Издатель токена (кто его выдал).
    /// Зачем нужно: Для проверки подлинности токена на стороне клиента и сервера.
    /// </summary>
    [Required(ErrorMessage = "Издатель JWT является обязательным.")]
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Аудитория токена (для кого предназначен токен).
    /// Зачем нужно: Для проверки подлинности токена на стороне клиента и сервера.
    /// </summary>
    [Required(ErrorMessage = "Аудитория JWT является обязательной.")]
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Срок действия Access Token в минутах.
    /// Зачем нужно: Для ограничения времени жизни Access Token, повышая безопасность.
    /// </summary>
    [Range(1, 120, ErrorMessage = "Срок действия Access Token должен быть от 1 до 120 минут.")]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Срок действия Refresh Token в днях.
    /// Зачем нужно: Для длительного поддержания сессии пользователя без повторного входа.
    /// </summary>
    [Range(1, 365, ErrorMessage = "Срок действия Refresh Token должен быть от 1 до 365 дней.")]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}