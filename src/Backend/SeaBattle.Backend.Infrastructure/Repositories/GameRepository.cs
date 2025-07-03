using Microsoft.EntityFrameworkCore;
using SeaBattle.Backend.Domain.Models;
using SeaBattle.Backend.Domain.Repositories;
using SeaBattle.Backend.Infrastructure.Data;

namespace SeaBattle.Backend.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с сущностями <see cref="Game"/>, используя Entity Framework Core.
/// </summary>
public class GameRepository : IGameRepository
{
    private readonly SeaBattleDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="GameRepository"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных для работы с играми.</param>
    public GameRepository(SeaBattleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Асинхронно получает игру по её уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор игры (GUID).</param>
    /// <returns>
    /// Объект <see cref="Game"/>, если найден, иначе <see langword="null"/>.
    /// </returns>
    public async Task<Game?> GetByIdAsync(Guid id)
    {
        // FindAsync ищет сущность по первичному ключу.
        // При необходимости загрузки связанных данных (Player1, Player2, Boards, Ships),
        // используйте .Include() в сервисе или выносите в отдельные методы репозитория,
        // чтобы избежать избыточной загрузки данных.
        return await _context.Games.FindAsync(id);
    }

    /// <summary>
    /// Асинхронно добавляет новую игру в хранилище.
    /// </summary>
    /// <param name="game">Объект <see cref="Game"/> для добавления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task AddAsync(Game game) // Убран async, так как нет await
    {
        // Даты Created и Modified устанавливаются автоматически в SeaBattleDbContext.
        _context.Games.Add(game); // Используем синхронный Add
        return Task.CompletedTask; // Возвращаем завершенную задачу
    }

    /// <summary>
    /// Асинхронно обновляет существующую игру в хранилище.
    /// </summary>
    /// <param name="game">Объект <see cref="Game"/> для обновления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task UpdateAsync(Game game) // Убран async, так как нет await
    {
        // Дата Modified устанавливается автоматически в SeaBattleDbContext.
        _context.Games.Update(game);
        return Task.CompletedTask; // Возвращаем завершенную задачу
    }

    /// <summary>
    /// Асинхронно удаляет игру из хранилища.
    /// </summary>
    /// <param name="game">Объект <see cref="Game"/> для удаления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task DeleteAsync(Game game) // Убран async, так как нет await
    {
        _context.Games.Remove(game);
        return Task.CompletedTask; // Возвращаем завершенную задачу
    }

    /// <summary>
    /// Асинхронно получает все игры, в которых участвовал конкретный пользователь.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор пользователя (GUID).</param>
    /// <returns>Коллекция объектов <see cref="Game"/>, в которых участвовал пользователь.</returns>
    public async Task<IEnumerable<Game>> GetGamesForUserAsync(Guid userId)
    {
        return await _context.Games
            .Where(g => g.Player1Id == userId || g.Player2Id == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Асинхронно получает все активные игры (со статусом <see cref="GameStatus.InProgress"/>).
    /// </summary>
    /// <returns>Коллекция активных объектов <see cref="Game"/>.</returns>
    public async Task<IEnumerable<Game>> GetActiveGamesAsync()
    {
        return await _context.Games
            .Where(g => g.Status == GameStatus.InProgress)
            .ToListAsync();
    }
}