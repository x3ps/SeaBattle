using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SeaBattle.Backend.Application.Interfaces;
using SeaBattle.Backend.Domain.Configuration;
using SeaBattle.Backend.Domain.Models;
using SeaBattle.Backend.Application.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace SeaBattle.Backend.Application.Services;

/// <summary>
/// Реализация <see cref="IJwtTokenGenerator"/> для создания Access Token и Refresh Token.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="JwtTokenGenerator"/>.
    /// </summary>
    /// <param name="jwtSettingsOptions">Опции конфигурации JWT.</param>
    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettingsOptions)
    {
        _jwtSettings = jwtSettingsOptions.Value;
    }

    /// <inheritdoc/>
    public string GenerateAccessToken(User user, IEnumerable<Claim>? claims = null)
    {
        // Список утверждений (claims), которые будут включены в токен
        var claimsList = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject - ID пользователя
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID - уникальный ID токена
            new Claim(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience), // Аудитория
            new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer), // Издатель
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Идентификатор пользователя
            new Claim(ClaimTypes.Name, user.Name) // Имя пользователя
            // Можно добавить другие claims, например, роли: new Claim(ClaimTypes.Role, "Admin")
        };

        if (claims != null)
        {
            claimsList.AddRange(claims);
        }

        // Секретный ключ для подписи токена
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );

        // Срок действия Access Token
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        // Создание JWT
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimsList),
            Expires = expires,
            SigningCredentials = signingCredentials,
            Audience = _jwtSettings.Audience,
            Issuer = _jwtSettings.Issuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    /// <inheritdoc/>
    public RefreshToken GenerateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), // Генерируем случайную строку
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays), // Срок действия из настроек
            Created = DateTime.UtcNow // Устанавливаем время создания (хотя EntityBase тоже это сделает)
        };

        return refreshToken;
    }

    /// <inheritdoc/>
    public Task<AuthResponse> GenerateAuthTokensAsync(User user, string ipAddress, IEnumerable<Claim>? claims = null)
    {
        // 1. Генерируем Access Token
        var accessToken = GenerateAccessToken(user, claims);
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        // 2. Генерируем Refresh Token
        var refreshToken = GenerateRefreshToken();
        refreshToken.UserId = user.Id; // Привязываем Refresh Token к пользователю
        refreshToken.CreatedByIp = ipAddress; // Записываем IP адрес создания

        // 3. Формируем AuthResponse
        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token, // Возвращаем строковое значение токена
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshToken.Expires,
            Username = user.Name,
            UserId = user.Id
        };
        
        return Task.FromResult(authResponse);
    }
}