using SeaBattle.Backend.Domain.Models;

namespace SeaBattle.Backend.Domain.Repositories;

public interface IGameRepository
{
    /// <summary>
    /// Получает игру по её уникальному идентификатору.
    /// </summary>
    /// <param name="id">ID игры.</param>
    /// <returns>Объект Game, если найден, иначе null.</returns>
    Task<Game?> GetByIdAsync(Guid id);

    /// <summary>
    /// Добавляет новую игру в хранилище.
    /// </summary>
    /// <param name="game">Объект игры для добавления.</param>
    Task AddAsync(Game game);

    /// <summary>
    /// Обновляет существующую игру в хранилище.
    /// </summary>
    /// <param name="game">Объект игры для обновления.</param>
    Task UpdateAsync(Game game);

    /// <summary>
    /// Удаляет игру из хранилища.
    /// </summary>
    /// <param name="game">Объект игры для удаления.</param>
    Task DeleteAsync(Game game);

    /// <summary>
    /// Получает все игры, в которых участвовал конкретный пользователь (как Player1 или Player2).
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <returns>Коллекция игр пользователя.</returns>
    Task<IEnumerable<Game>> GetGamesForUserAsync(Guid userId);

    /// <summary>
    /// Получает активные игры (со статусом InProgress).
    /// </summary>
    /// <returns>Коллекция активных игр.</returns>
    Task<IEnumerable<Game>> GetActiveGamesAsync();
}