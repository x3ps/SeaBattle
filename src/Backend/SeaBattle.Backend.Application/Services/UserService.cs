using Microsoft.Extensions.Logging;
using SeaBattle.Backend.Application.DTOs.Auth;
using SeaBattle.Backend.Application.Interfaces;
using SeaBattle.Backend.Domain.Models;
using SeaBattle.Backend.Domain.Repositories;
using SeaBattle.Backend.Domain.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http; // УДАЛЯЕМ эту зависимость!
// using System.Net; // УДАЛЯЕМ эту зависимость!

namespace SeaBattle.Backend.Application.Services;

/// <summary>
/// Реализация сервиса для управления пользователями и аутентификацией.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<UserService> _logger;
    // private readonly IHttpContextAccessor _httpContextAccessor; // УДАЛЯЕМ это поле!

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="UserService"/>.
    /// </summary>
    /// <param name="unitOfWork">Единица работы для координации репозиториев.</param>
    /// <param name="passwordHasher">Хешер паролей.</param>
    /// <param name="jwtTokenGenerator">Генератор JWT токенов.</param>
    /// <param name="logger">Логгер.</param>
    // Удаляем IHttpContextAccessor из конструктора
    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
        // _httpContextAccessor = httpContextAccessor; // УДАЛЯЕМ эту строку!
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> RegisterUserAsync(RegisterRequest request, string ipAddress) // Принимаем ipAddress
    {
        _logger.LogInformation("Попытка регистрации нового пользователя: {Username} с IP: {IpAddress}", request.Username, ipAddress);

        var existingUser = await _unitOfWork.Users.GetByNameAsync(request.Username);
        if (existingUser != null)
        {
            _logger.LogWarning("Пользователь с именем '{Username}' уже существует.", request.Username);
            return null;
        }

        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Username,
            PasswordHash = hashedPassword,
            Wins = 0,
            Losses = 0
        };

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Пользователь '{Username}' успешно зарегистрирован. ID: {UserId}", newUser.Name, newUser.Id);

        var authResponse = await _jwtTokenGenerator.GenerateAuthTokensAsync(newUser, ipAddress); // Передаем ipAddress

        if (authResponse?.RefreshToken != null)
        {
            var newRefreshTokenObject = new RefreshToken
            {
                Token = authResponse.RefreshToken,
                Expires = authResponse.RefreshTokenExpiry,
                UserId = newUser.Id,
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress // Используем переданный ipAddress
            };
            await _unitOfWork.Users.AddRefreshTokenAsync(newUser.Id, newRefreshTokenObject);
            await _unitOfWork.SaveChangesAsync();
        }

        return authResponse;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> AuthenticateUserAsync(LoginRequest request, string ipAddress) // Принимаем ipAddress
    {
        _logger.LogInformation("Попытка аутентификации пользователя: {Username} с IP: {IpAddress}", request.Username, ipAddress);

        var user = await _unitOfWork.Users.GetByNameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Пользователь с именем '{Username}' не найден.", request.Username);
            return null;
        }

        var passwordVerificationResult = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);
        if (!passwordVerificationResult)
        {
            _logger.LogWarning("Неверный пароль для пользователя: {Username}", request.Username);
            return null;
        }

        _logger.LogInformation("Пользователь '{Username}' успешно аутентифицирован.", request.Username);

        var authResponse = await _jwtTokenGenerator.GenerateAuthTokensAsync(user, ipAddress); // Передаем ipAddress

        if (authResponse?.RefreshToken != null)
        {
            var newRefreshTokenObject = new RefreshToken
            {
                Token = authResponse.RefreshToken,
                Expires = authResponse.RefreshTokenExpiry,
                UserId = user.Id,
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress // Используем переданный ipAddress
            };
            await _unitOfWork.Users.AddRefreshTokenAsync(user.Id, newRefreshTokenObject);
            await _unitOfWork.SaveChangesAsync();
        }

        return authResponse;
    }

    /// <inheritdoc/>
    public async Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        _logger.LogInformation("Попытка смены пароля для пользователя ID: {UserId}", userId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("Пользователь с ID {UserId} не найден для смены пароля.", userId);
            return new ChangePasswordResponse { Success = false, Message = "Пользователь не найден." };
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            _logger.LogWarning("Неверный текущий пароль для пользователя ID: {UserId}", userId);
            return new ChangePasswordResponse { Success = false, Message = "Неверный текущий пароль." };
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            return new ChangePasswordResponse { Success = false, Message = "Новый пароль не может совпадать с текущим." };
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Пароль для пользователя ID: {UserId} успешно изменен.", userId);
        return new ChangePasswordResponse { Success = true, Message = "Пароль успешно изменен." };
    }

    /// <inheritdoc/>
    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        _logger.LogInformation("Попытка обновления токена для IP: {IpAddress}", request.IpAddress);

        var refreshToken = await _unitOfWork.Users.GetRefreshTokenByTokenAsync(request.RefreshToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Невалидный Refresh Token (не найден). IP: {IpAddress}", request.IpAddress);
            return null;
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Отозванный Refresh Token использован. Token: {Token}, IP: {IpAddress}", request.RefreshToken, request.IpAddress);

            if (refreshToken.User != null)
            {
                await RevokeAllUserRefreshTokensAsync(refreshToken.User.Id, RevokeReason.Compromised);
                await _unitOfWork.SaveChangesAsync();
            }
            return null;
        }

        if (refreshToken.IsExpired)
        {
            _logger.LogWarning("Истекший Refresh Token использован. Token: {Token}, IP: {IpAddress}", request.RefreshToken, request.IpAddress);
            return null;
        }

        if (refreshToken.ReplacedByToken != null)
        {
            _logger.LogWarning("Refresh Token уже был заменен. Token: {Token}, IP: {IpAddress}", request.RefreshToken, request.IpAddress);
            return null;
        }

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = request.IpAddress;
        refreshToken.ReasonRevoked = RevokeReason.ReplacedByNewToken;

        var user = refreshToken.User;
        if (user == null)
        {
            _logger.LogError("Refresh Token найден, но связанный пользователь не загружен. Token: {Token}", request.RefreshToken);
            return null;
        }

        var newAuthResponse = await _jwtTokenGenerator.GenerateAuthTokensAsync(user, request.IpAddress);

        var newRefreshTokenObject = new RefreshToken
        {
            Token = newAuthResponse.RefreshToken,
            Expires = newAuthResponse.RefreshTokenExpiry,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            CreatedByIp = request.IpAddress
        };

        refreshToken.ReplacedByToken = newRefreshTokenObject.Token;

        await _unitOfWork.Users.AddRefreshTokenAsync(user.Id, newRefreshTokenObject);
        await _unitOfWork.Users.UpdateRefreshTokenAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refresh Token успешно обновлен для пользователя: {Username}, IP: {IpAddress}", user.Name, request.IpAddress);
        return newAuthResponse;
    }

    /// <inheritdoc/>
    public async Task<RevokeTokenResponse> RevokeRefreshTokenAsync(RevokeTokenRequest request)
    {
        _logger.LogInformation("Попытка отзыва токена: {Token} для IP: {IpAddress}", request.RefreshToken, request.IpAddress);

        var refreshToken = await _unitOfWork.Users.GetRefreshTokenByTokenAsync(request.RefreshToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Попытка отзыва невалидного токена (не найден): {Token}", request.RefreshToken);
            return new RevokeTokenResponse { Success = false, Message = "Токен не найден." };
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Попытка отзыва уже отозванного токена: {Token}", request.RefreshToken);
            return new RevokeTokenResponse { Success = false, Message = "Токен уже отозван." };
        }

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = request.IpAddress;
        RevokeReason parsedReason;
        if (Enum.TryParse(request.Reason, true, out parsedReason))
        {
            refreshToken.ReasonRevoked = parsedReason;
        }
        else
        {
            refreshToken.ReasonRevoked = RevokeReason.Manual;
        }

        await _unitOfWork.Users.UpdateRefreshTokenAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refresh Token {Token} успешно отозван для IP: {IpAddress}", request.RefreshToken, request.IpAddress);
        return new RevokeTokenResponse { Success = true, Message = "Токен успешно отозван." };
    }

    /// <inheritdoc/>
    public async Task<bool> IncrementUserWinsAsync(Guid userId)
    {
        _logger.LogInformation("Увеличение побед для пользователя ID: {UserId}", userId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Пользователь с ID {UserId} не найден для увеличения побед.", userId);
            return false;
        }

        user.Wins++;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Победы пользователя ID: {UserId} увеличены до {Wins}", userId, user.Wins);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> IncrementUserLossesAsync(Guid userId)
    {
        _logger.LogInformation("Увеличение поражений для пользователя ID: {UserId}", userId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Пользователь с ID {UserId} не найден для увеличения поражений.", userId);
            return false;
        }

        user.Losses++;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Поражения пользователя ID: {UserId} увеличены до {Losses}", userId, user.Losses);
        return true;
    }

    /// <summary>
    /// Вспомогательный метод для отзыва всех токенов пользователя.
    /// Используется при обнаружении подозрительной активности (например, использование отозванного токена).
    /// </summary>
    /// <param name="userId">ID пользователя, чьи токены нужно отозвать.</param>
    /// <param name="reason">Причина отзыва.</param>
    private async Task RevokeAllUserRefreshTokensAsync(Guid userId, RevokeReason reason)
    {
        _logger.LogWarning("Отзыв всех Refresh Tokens для пользователя ID: {UserId} по причине: {Reason}", userId, reason);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user?.RefreshTokens != null)
        {
            foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
            {
                token.Revoked = DateTime.UtcNow;
                token.ReasonRevoked = reason;
                await _unitOfWork.Users.UpdateRefreshTokenAsync(token);
            }
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Все активные Refresh Tokens для пользователя ID: {UserId} отозваны.", userId);
        }
    }
}