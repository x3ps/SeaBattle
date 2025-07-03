namespace SeaBattle.Backend.Domain.Models;

/// <summary>
/// Представляет игровое поле одного игрока в конкретной игре.
/// </summary>
public class Board : EntityBase
{
    /// <summary>
    /// ID игры, которой принадлежит это поле.
    /// Зачем нужно: Внешний ключ для связи с сущностью Game.
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// Навигационное свойство для игры, которой принадлежит поле.
    /// Зачем нужно: Позволяет EF Core загружать данные Game вместе с Board.
    /// </summary>
    public Game Game { get; set; } = null!;

    /// <summary>
    /// ID пользователя, которому принадлежит это поле.
    /// Зачем нужно: Внешний ключ для связи с сущностью User, определяющий владельца поля.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Навигационное свойство для игрока, которому принадлежит поле.
    /// Зачем нужно: Позволяет EF Core загружать данные User вместе с Board.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Коллекция кораблей, размещенных на этом поле.
    /// Зачем нужно: Для отслеживания положения, типа и состояния всех кораблей на поле игрока.
    /// </summary>
    public ICollection<Ship> Ships { get; set; } = new List<Ship>();

    /// <summary>
    /// Коллекция всех попаданий (выстрелов), сделанных по этому полю.
    /// Зачем нужно: Для отслеживания обстрелянных клеток и их результатов (попадания/промахи).
    /// </summary>
    public ICollection<BoardHit> Hits { get; set; } = new List<BoardHit>();
}