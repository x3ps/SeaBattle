namespace SeaBattle.Backend.Domain.Models;

/// <summary>
/// Представляет игровую сессию в "Морском бое".
/// </summary>
public class Game : EntityBase
{
    /// <summary>
    /// Время начала игры.
    /// Зачем нужно: Для отслеживания продолжительности игры и ведения истории.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Время окончания игры (может быть null, если игра еще не завершена).
    /// Зачем нужно: Для отслеживания продолжительности игры, когда она завершится.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// ID первого игрока.
    /// Зачем нужно: Внешний ключ для связи с сущностью User, представляющий первого участника игры.
    /// </summary>
    public Guid Player1Id { get; set; }

    /// <summary>
    /// Навигационное свойство для первого игрока.
    /// Зачем нужно: Позволяет EF Core загружать данные Player1 вместе с Game.
    /// </summary>
    public User Player1 { get; set; } = null!;

    /// <summary>
    /// ID второго игрока.
    /// Зачем нужно: Внешний ключ для связи с сущностью User, представляющий второго участника игры.
    /// </summary>
    public Guid Player2Id { get; set; }

    /// <summary>
    /// Навигационное свойство для второго игрока.
    /// Зачем нужно: Позволяет EF Core загружать данные Player2 вместе с Game.
    /// </summary>
    public User Player2 { get; set; } = null!;

    /// <summary>
    /// ID победившего игрока (может быть null, если игра не завершена или отменена).
    /// Зачем нужно: Для определения результата игры и обновления статистики игрока.
    /// </summary>
    public Guid? WinnerId { get; set; }

    /// <summary>
    /// Навигационное свойство для победившего игрока.
    /// Зачем нужно: Позволяет EF Core загружать данные Winner вместе с Game.
    /// </summary>
    public User? Winner { get; set; }

    /// <summary>
    /// Текущий статус игры.
    /// Зачем нужно: Для отслеживания состояния игры (в процессе, завершена, отменена).
    /// </summary>
    public GameStatus Status { get; set; }
}

/// <summary>
/// Перечисление возможных статусов игры.
/// </summary>
public enum GameStatus
{
    InProgress, // Игра активна и продолжается
    Completed,  // Игра завершена (есть победитель/проигравший)
    Canceled    // Игра была отменена (например, один из игроков вышел)
}