using SeaBattle.Backend.Domain.Models;

namespace SeaBattle.Backend.Application.Interfaces;


public interface IGameService
{
    /// <summary>
    /// Создает новую игру между двумя игроками.
    /// </summary>
    /// <param name="player1Id">ID первого игрока.</param>
    /// <param name="player2Id">ID второго игрока.</param>
    /// <returns>Созданный объект Game, или null, если игроки не найдены или возникла другая ошибка.</returns>
    Task<Game?> CreateGameAsync(Guid player1Id, Guid player2Id);

    /// <summary>
    /// Получает игру по её ID.
    /// </summary>
    /// <param name="gameId">ID игры.</param>
    /// <returns>Объект Game, если найден, иначе null.</returns>
    Task<Game?> GetGameByIdAsync(Guid gameId);

    /// <summary>
    /// Отменяет игру.
    /// </summary>
    /// <param name="gameId">ID игры для отмены.</param>
    /// <returns>True, если игра успешно отменена, иначе false.</returns>
    Task<bool> CancelGameAsync(Guid gameId);

    /// <summary>
    /// Завершает игру и объявляет победителя.
    /// </summary>
    /// <param name="gameId">ID завершаемой игры.</param>
    /// <param name="winnerId">ID победившего игрока.</param>
    /// <returns>True, если игра успешно завершена, иначе false.</returns>
    Task<bool> CompleteGameAsync(Guid gameId, Guid winnerId);

    /// <summary>
    /// Выполняет выстрел по игровому полю.
    /// </summary>
    /// <param name="gameId">ID игры.</param>
    /// <param name="shootingPlayerId">ID игрока, который делает выстрел.</param>
    /// <param name="targetPlayerId">ID игрока, по полю которого производится выстрел.</param>
    /// <param name="x">Координата X выстрела.</param>
    /// <param name="y">Координата Y выстрела.</param>
    /// <returns>Объект BoardHit, описывающий результат выстрела, или null, если выстрел невозможен.</returns>
    Task<BoardHit?> MakeShotAsync(Guid gameId, Guid shootingPlayerId, Guid targetPlayerId, int x, int y);

    /// <summary>
    /// Добавляет корабль на поле игрока.
    /// </summary>
    /// <param name="gameId">ID игры, в которой размещается корабль.</param>
    /// <param name="userId">ID пользователя, которому принадлежит поле.</param>
    /// <param name="shipType">Тип корабля.</param>
    /// <param name="startX">Начальная координата X корабля.</param>
    /// <param name="startY">Начальная координата Y корабля.</param>
    /// <param name="orientation">Ориентация корабля (горизонтальная/вертикальная).</param>
    /// <returns>Созданный объект Ship, или null, если размещение невозможно (например, коллизия, выход за границы).</returns>
    Task<Ship?> PlaceShipAsync(Guid gameId, Guid userId, ShipType shipType, int startX, int startY, ShipOrientation orientation);

    /// <summary>
    /// Получает игровое поле для конкретного игрока в конкретной игре.
    /// </summary>
    /// <param name="gameId">ID игры.</param>
    /// <param name="userId">ID пользователя, чье поле нужно получить.</param>
    /// <returns>Объект Board, если найден, иначе null.</returns>
    Task<Board?> GetPlayerBoardAsync(Guid gameId, Guid userId);

    /// <summary>
    /// Проверяет, все ли корабли игрока потоплены.
    /// </summary>
    /// <param name="gameId">ID игры.</param>
    /// <param name="userId">ID игрока.</param>
    /// <returns>True, если все корабли игрока потоплены, иначе false.</returns>
    Task<bool> AreAllShipsSunkAsync(Guid gameId, Guid userId);
}