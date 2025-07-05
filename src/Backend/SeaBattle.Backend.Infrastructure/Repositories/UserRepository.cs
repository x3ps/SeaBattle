using Microsoft.EntityFrameworkCore;
using SeaBattle.Backend.Domain.Models;
using SeaBattle.Backend.Domain.Repositories;
using SeaBattle.Backend.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    /// Асинхронно получает пользователя по его уникальному идентификатору,
    /// включая его токены обновления.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя (GUID).</param>
    /// <returns>
    /// Объект <see cref="User"/>, если найден, иначе <see langword="null"/>.
    /// </returns>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
                             .Include(u => u.RefreshTokens)
                             .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Асинхронно получает пользователя по его имени,
    /// включая его токены обновления.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <returns>
    /// Объект <see cref="User"/>, если найден, иначе <see langword="null"/>.
    /// </returns>
    public async Task<User?> GetByNameAsync(string name)
    {
        return await _context.Users
                             .Include(u => u.RefreshTokens)
                             .FirstOrDefaultAsync(u => u.Name == name);
    }

    /// <summary>
    /// Асинхронно добавляет нового пользователя в хранилище.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для добавления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    /// <summary>
    /// Асинхронно обновляет существующего пользователя в хранилище.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для обновления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Асинхронно удаляет пользователя из хранилища.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для удаления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Асинхронно получает коллекцию всех пользователей.
    /// </summary>
    /// <returns>
    /// Коллекция объектов <see cref="User"/>.
    /// </returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.Include(u => u.RefreshTokens).ToListAsync();
    }

    /// <summary>
    /// Асинхронно добавляет новый токен обновления для указанного пользователя.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор пользователя.</param>
    /// <param name="refreshToken">Объект <see cref="RefreshToken"/> для добавления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task AddRefreshTokenAsync(Guid userId, RefreshToken refreshToken)
    {
        refreshToken.UserId = userId;
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    /// <summary>
    /// Асинхронно получает токен обновления по его строковому значению.
    /// </summary>
    /// <param name="token">Строковое значение токена обновления.</param>
    /// <returns>Объект <see cref="RefreshToken"/>, если найден, иначе <see langword="null"/>.</returns>
    public async Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token)
    {
        return await _context.RefreshTokens
                             .Include(rt => rt.User)
                             .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <summary>
    /// Асинхронно удаляет указанный токен обновления из хранилища.
    /// </summary>
    /// <param name="refreshToken">Объект <see cref="RefreshToken"/> для удаления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task DeleteRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Асинхронно обновляет существующий токен обновления в хранилище.
    /// </summary>
    /// <param name="refreshToken">Объект <see cref="RefreshToken"/> для обновления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }
}