using SeaBattle.Backend.Domain.Models;
using SeaBattle.Backend.Application.DTOs.Auth;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace SeaBattle.Backend.Application.Interfaces;

/// <summary>
/// Определяет контракт для генерации JSON Web Tokens (JWT)
/// и токенов обновления (Refresh Tokens).
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Генерирует Access Token (JWT) для указанного пользователя.
    /// Access Token предназначен для аутентификации запросов к API.
    /// </summary>
    /// <param name="user">Объект пользователя, для которого генерируется токен.</param>
    /// <param name="claims">Список дополнительных claims (утверждений), которые будут включены в токен.</param>
    /// <returns>Строковое представление сгенерированного Access Token.</returns>
    string GenerateAccessToken(User user, IEnumerable<Claim>? claims = null);

    /// <summary>
    /// Генерирует новый Refresh Token.
    /// </summary>
    /// <returns>Новый объект RefreshToken с сгенерированным токеном и сроком действия.</returns>
    RefreshToken GenerateRefreshToken();

    /// <summary>
    /// Генерирует полный объект AuthResponse, содержащий Access Token и Refresh Token,
    /// а также их сроки действия, для указанного пользователя.
    /// </summary>
    /// <param name="user">Объект пользователя, для которого генерируется ответ.</param>
    /// <param name="ipAddress">IP-адрес, с которого был сделан запрос, для записи в RefreshToken.</param>
    /// <param name="claims">Список дополнительных claims (утверждений) для Access Token.</param>
    /// <returns>Объект <see cref="AuthResponse"/>, содержащий сгенерированные токены и информацию о пользователе.</returns>
    Task<AuthResponse> GenerateAuthTokensAsync(User user, string ipAddress, IEnumerable<Claim>? claims = null);
}