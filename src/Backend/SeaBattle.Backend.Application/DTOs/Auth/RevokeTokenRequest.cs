namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Запрос на отзыв (деактивацию) Refresh Token.
/// </summary>
public class RevokeTokenRequest
{
    /// <summary>
    /// Refresh Token, который нужно отозвать.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// IP-адрес клиента, с которого поступил запрос на отзыв.
    /// Используется для логирования и безопасности.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Необязательная причина отзыва токена.
    /// </summary>
    public string? Reason { get; set; }
}