using Microsoft.EntityFrameworkCore;
using SeaBattle.Backend.Domain.Models;
using SeaBattle.Backend.Domain.Repositories;
using SeaBattle.Backend.Infrastructure.Data;

namespace SeaBattle.Backend.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с сущностями <see cref="User"/>, используя Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly SeaBattleDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных для работы с пользователями.</param>
    public UserRepository(SeaBattleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Асинхронно получает пользователя по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя (GUID).</param>
    /// <returns>
    /// Объект <see cref="User"/>, если найден, иначе <see langword="null"/>.
    /// </returns>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        // FindAsync ищет сущность по первичному ключу.
        return await _context.Users.FindAsync(id);
    }

    /// <summary>
    /// Асинхронно получает пользователя по его имени.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <returns>
    /// Объект <see cref="User"/>, если найден, иначе <see langword="null"/>.
    /// </returns>
    public async Task<User?> GetByNameAsync(string name)
    {
        // FirstOrDefaultAsync возвращает первый элемент, удовлетворяющий условию,
        // или null, если таких элементов не найдено.
        return await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
    }

    /// <summary>
    /// Асинхронно добавляет нового пользователя в хранилище.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для добавления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task AddAsync(User user) // Убран async, так как нет await
    {
        // Даты Created и Modified устанавливаются автоматически в SeaBattleDbContext.
        _context.Users.Add(user); // Используем синхронный Add
        return Task.CompletedTask; // Возвращаем завершенную задачу
    }

    /// <summary>
    /// Асинхронно обновляет существующего пользователя в хранилище.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для обновления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task UpdateAsync(User user) // Убран async, так как нет await
    {
        // Дата Modified устанавливается автоматически в SeaBattleDbContext.
        _context.Users.Update(user);
        return Task.CompletedTask; // Возвращаем завершенную задачу
    }

    /// <summary>
    /// Асинхронно удаляет пользователя из хранилища.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для удаления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task DeleteAsync(User user) // Убран async, так как нет await
    {
        _context.Users.Remove(user);
        return Task.CompletedTask; // Возвращаем завершенную задачу
    }

    /// <summary>
    /// Асинхронно получает коллекцию всех пользователей.
    /// </summary>
    /// <returns>
    /// Коллекция объектов <see cref="User"/>.
    /// </returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
}