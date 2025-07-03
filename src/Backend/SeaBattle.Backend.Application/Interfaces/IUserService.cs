using SeaBattle.Backend.Domain.Models;

namespace SeaBattle.Backend.Application.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <param name="password">Пароль пользователя.</param>
    /// <returns>Созданный объект User, или null, если пользователь с таким именем уже существует.</returns>
    Task<User?> RegisterUserAsync(string name, string password);

    /// <summary>
    /// Выполняет аутентификацию пользователя.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <param name="password">Пароль пользователя.</param>
    /// <returns>Объект User, если аутентификация успешна, иначе null.</returns>
    Task<User?> AuthenticateUserAsync(string name, string password);

    /// <summary>
    /// Изменяет пароль пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <param name="currentPassword">Текущий пароль.</param>
    /// <param name="newPassword">Новый пароль.</param>
    /// <returns>True, если пароль успешно изменен, иначе false (например, если текущий пароль неверный или пользователь не найден).</returns>
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

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