namespace SeaBattle.Backend.Domain.Models
{
    /// <summary>
    /// Представляет токен обновления (Refresh Token) для аутентификации пользователя.
    /// </summary>
    public class RefreshToken : EntityBase
    {
        /// <summary>
        /// Уникальная строка токена обновления.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время истечения срока действия токена.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// IP-адрес, с которого был создан токен.
        /// </summary>
        public string CreatedByIp { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время отзыва токена (если он был отозван).
        /// </summary>
        public DateTime? Revoked { get; set; }

        /// <summary>
        /// IP-адрес, с которого был отозван токен.
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Токен, которым был заменен данный токен (для ротации токенов).
        /// </summary>
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Причина отзыва токена.
        /// </summary>
        public RevokeReason? ReasonRevoked { get; set; }

        /// <summary>
        /// Указывает, отозван ли токен.
        /// </summary>
        public bool IsRevoked => Revoked != null; // Добавлено вычисляемое свойство

        /// <summary>
        /// Указывает, истек ли срок действия токена.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= Expires;

        /// <summary>
        /// Указывает, активен ли токен в данный момент (не отозван, не истек и не заменен).
        /// </summary>
        // Обновим IsActive, чтобы учитывать ReplacedByToken
        public bool IsActive => !IsRevoked && !IsExpired && ReplacedByToken == null;


        /// <summary>
        /// Внешний ключ, связывающий токен обновления с пользователем.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Навигационное свойство для связанного пользователя.
        /// </summary>
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Перечисление возможных причин отзыва Refresh Token.
    /// </summary>
    public enum RevokeReason
    {
        /// <summary>
        /// Токен был отозван вручную администратором или по запросу пользователя.
        /// </summary>
        Manual = 0,

        /// <summary>
        /// Токен истек по сроку действия.
        /// </summary>
        Expired = 1,

        /// <summary>
        /// Токен был заменен новым токеном в процессе ротации.
        /// </summary>
        ReplacedByNewToken = 2,

        /// <summary>
        /// Токен был скомпрометирован.
        /// </summary>
        Compromised = 3,

        /// <summary>
        /// Неизвестная или неклассифицированная причина отзыва.
        /// </summary>
        Unknown = 4
    }
}