using SeaBattle.Backend.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeaBattle.Backend.Domain.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Асинхронно получает пользователя по его уникальному идентификатору,
    /// включая его токены обновления.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя (GUID).</param>
    /// <returns>Объект <see cref="User"/>, если найден, иначе <see langword="null"/>.</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Асинхронно получает пользователя по его имени,
    /// включая его токены обновления.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <returns>Объект <see cref="User"/>, если найден, иначе <see langword="null"/>.</returns>
    Task<User?> GetByNameAsync(string name);

    /// <summary>
    /// Асинхронно добавляет нового пользователя в хранилище.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для добавления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task AddAsync(User user);

    /// <summary>
    /// Асинхронно обновляет существующего пользователя в хранилище.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для обновления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task UpdateAsync(User user);

    /// <summary>
    /// Асинхронно удаляет пользователя из хранилища.
    /// </summary>
    /// <param name="user">Объект <see cref="User"/> для удаления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task DeleteAsync(User user);

    /// <summary>
    /// Асинхронно получает коллекцию всех пользователей.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="User"/>.</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Асинхронно добавляет новый токен обновления для указанного пользователя.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор пользователя.</param>
    /// <param name="refreshToken">Объект <see cref="RefreshToken"/> для добавления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task AddRefreshTokenAsync(Guid userId, RefreshToken refreshToken);

    /// <summary>
    /// Асинхронно получает токен обновления по его строковому значению.
    /// </summary>
    /// <param name="token">Строковое значение токена обновления.</param>
    /// <returns>Объект <see cref="RefreshToken"/>, если найден, иначе <see langword="null"/>.</returns>
    Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token);

    /// <summary>
    /// Асинхронно удаляет указанный токен обновления из хранилища.
    /// </summary>
    /// <param name="refreshToken">Объект <see cref="RefreshToken"/> для удаления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task DeleteRefreshTokenAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Асинхронно обновляет существующий токен обновления в хранилище.
    /// </summary>
    /// <param name="refreshToken">Объект <see cref="RefreshToken"/> для обновления.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken); // Добавлено
}