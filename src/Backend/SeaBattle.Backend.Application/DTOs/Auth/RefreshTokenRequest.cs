namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Запрос на обновление Access Token с использованием Refresh Token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Текущий Refresh Token, полученный от клиента.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// IP-адрес клиента, с которого поступил запрос.
    /// Используется для логирования и безопасности.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}