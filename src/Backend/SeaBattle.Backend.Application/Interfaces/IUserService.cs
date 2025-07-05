using SeaBattle.Backend.Application.DTOs.Auth;
using SeaBattle.Backend.Domain.Models; // Убедись, что этот using есть

namespace SeaBattle.Backend.Application.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Регистрирует нового пользователя в системе.
    /// </summary>
    /// <param name="request">Данные для регистрации пользователя (Username, Password).</param>
    /// <returns>AuthResponse с токенами и информацией о пользователе, если регистрация успешна, иначе null.</returns>
    Task<AuthResponse?> RegisterUserAsync(RegisterRequest request, string ipAddress);

    /// <summary>
    /// Выполняет аутентификацию пользователя.
    /// </summary>
    /// <param name="request">Данные для входа пользователя (Username, Password).</param>
    /// <returns>AuthResponse с токенами и информацией о пользователе, если аутентификация успешна, иначе null.</returns>
    Task<AuthResponse?> AuthenticateUserAsync(LoginRequest request, string ipAddress);

    /// <summary>
    /// Изменяет пароль пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя, который меняет пароль.</param>
    /// <param name="request">Данные для смены пароля (CurrentPassword, NewPassword).</param>
    /// <returns>ChangePasswordResponse, информирующий об успехе или неудаче.</returns>
    Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    /// <summary>
    /// Обновляет Access Token с использованием Refresh Token и, возможно, ротирует Refresh Token.
    /// </summary>
    /// <param name="request">Запрос на обновление токена, содержащий Refresh Token и IP-адрес клиента.</param>
    /// <returns>AuthResponse с новыми токенами, если Refresh Token валиден и успешно обновлен, иначе null.</returns>
    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// Отзывает (делает недействительным) Refresh Token.
    /// </summary>
    /// <param name="request">Запрос на отзыв токена, содержащий Refresh Token и IP-адрес клиента.</param>
    /// <returns>RevokeTokenResponse, информирующий об успехе или неудаче отзыва токена.</returns>
    Task<RevokeTokenResponse> RevokeRefreshTokenAsync(RevokeTokenRequest request);

    /// <summary>
    /// Увеличивает количество побед пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <returns>True, если обновление успешно, иначе false.</returns>
    Task<bool> IncrementUserWinsAsync(Guid userId);

    /// <summary>
    /// Увеличивает количество поражений пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <returns>True, если обновление успешно, иначе false.</returns>
    Task<bool> IncrementUserLossesAsync(Guid userId);
}