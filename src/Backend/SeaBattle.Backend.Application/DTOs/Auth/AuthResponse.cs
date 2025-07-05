using System;

namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Представляет ответ об успешной аутентификации или регистрации пользователя.
/// Содержит токены доступа и обновления, а также основную информацию о пользователе.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Токен доступа (JWT), используемый для аутентифицированных запросов к API.
    /// Он обычно имеет короткий срок действия.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Токен обновления, используемый для получения нового AccessToken
    /// без повторной аутентификации. Он обычно имеет более длительный срок действия.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время истечения срока действия AccessToken (в UTC).
    /// </summary>
    public DateTime AccessTokenExpiry { get; set; }

    /// <summary>
    /// Дата и время истечения срока действия RefreshToken (в UTC).
    /// </summary>
    public DateTime RefreshTokenExpiry { get; set; }

    /// <summary>
    /// Имя пользователя, для которого был выдан токен.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный идентификатор пользователя, для которого был выдан токен.
    /// </summary>
    public Guid UserId { get; set; }
}