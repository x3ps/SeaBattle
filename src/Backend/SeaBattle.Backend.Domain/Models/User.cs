namespace SeaBattle.Backend.Domain.Models;

/// <summary>
/// Представляет пользователя в системе.
/// </summary>
public class User : EntityBase
{
    /// <summary>
    /// Имя пользователя (логин). Должно быть уникальным.
    /// </summary>
    public string Name { get; set; } = null!;
    

    /// <summary>
    /// Хешированный пароль пользователя. Никогда не хранится в открытом виде.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Токены обновления (Refresh Token) для продления сессии без повторной авторизации.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Количество побед пользователя.
    /// </summary>
    public int Wins { get; set; } = 0;

    /// <summary>
    /// Количество поражений пользователя.
    /// </summary>
    public int Losses { get; set; } = 0;
}