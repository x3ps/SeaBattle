namespace SeaBattle.Backend.Domain.Helpers;

/// <summary>
/// Определяет контракт для сервиса хеширования и проверки паролей.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хеширует переданный пароль.
    /// Зачем нужно: Безопасное хранение паролей в базе данных, предотвращая их утечку в открытом виде.
    /// </summary>
    /// <param name="password">Пароль для хеширования.</param>
    /// <returns>Хешированная строка пароля.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Проверяет, соответствует ли открытый пароль хешированному.
    /// Зачем нужно: Для аутентификации пользователя путем сравнения введенного пароля с сохраненным хешем, не раскрывая сам пароль.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="hashedPassword">Хешированный пароль из хранилища.</param>
    /// <returns>True, если пароли совпадают, иначе false.</returns>
    bool VerifyPassword(string password, string hashedPassword);
}