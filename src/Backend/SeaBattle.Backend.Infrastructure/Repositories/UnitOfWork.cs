using Microsoft.EntityFrameworkCore;
using SeaBattle.Backend.Domain.Repositories;
using SeaBattle.Backend.Infrastructure.Data;

namespace SeaBattle.Backend.Infrastructure.Repositories;

/// <summary>
/// Реализация паттерна Unit of Work, координирующего работу с репозиториями
/// и управляющего сохранением изменений в базе данных.
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly SeaBattleDbContext _context;
    private IUserRepository? _userRepository;
    private IGameRepository? _gameRepository;
    private bool _disposed = false; // Флаг для отслеживания состояния диспоуза

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="UnitOfWork"/>.
    /// </summary>
    /// <param name="context">Контекст базы данных, используемый для всех репозиториев в этой единице работы.</param>
    public UnitOfWork(SeaBattleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Предоставляет доступ к репозиторию пользователей.
    /// Репозиторий создается при первом обращении (ленивая инициализация).
    /// </summary>
    public IUserRepository Users
    {
        get
        {
            // Если репозиторий еще не создан, создаем его, передавая текущий DbContext
            _userRepository ??= new UserRepository(_context);
            return _userRepository;
        }
    }

    /// <summary>
    /// Предоставляет доступ к репозиторию игр.
    /// Репозиторий создается при первом обращении (ленивая инициализация).
    /// </summary>
    public IGameRepository Games
    {
        get
        {
            // Если репозиторий еще не создан, создаем его, передавая текущий DbContext
            _gameRepository ??= new GameRepository(_context);
            return _gameRepository;
        }
    }

    /// <summary>
    /// Асинхронно сохраняет все изменения, отслеживаемые в текущей единице работы, в базу данных.
    /// </summary>
    /// <returns>Количество измененных записей в базе данных.</returns>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Откатывает все изменения, отслеживаемые в текущей единице работы,
    /// путем сброса состояния всех отслеживаемых сущностей.
    /// Этот метод обычно вызывается при возникновении ошибок для отмены несохраненных изменений.
    /// </summary>
    public void Rollback()
    {
        // Для Entity Framework Core "откат" означает отмену изменений, которые не были сохранены.
        // Этого можно добиться, изменив состояние сущностей в Change Tracker на Unchanged.
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached; // Отсоединяем добавленные сущности
                    break;
                case EntityState.Modified:
                    entry.Reload(); // Перезагружаем оригинальные значения для измененных сущностей
                    entry.State = EntityState.Unchanged; // Сбрасываем состояние на "неизмененное"
                    break;
                case EntityState.Deleted:
                    entry.Reload(); // Перезагружаем сущность
                    // Это может быть сложнее, если сущность была удалена из базы.
                    // Для простоты, здесь просто возвращаем её в Unchanged, предполагая, что Reload сработает.
                    // В более сложных сценариях, возможно, потребуется более детальная логика.
                    entry.State = EntityState.Unchanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Освобождает неуправляемые ресурсы и опционально управляемые ресурсы.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> для освобождения управляемых и неуправляемых ресурсов;
    /// <see langword="false"/> для освобождения только неуправляемых ресурсов.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Освобождаем управляемые ресурсы, включая DbContext
                _context.Dispose();
            }
            // Освобождаем неуправляемые ресурсы, если таковые имеются
            _disposed = true;
        }
    }

    /// <summary>
    /// Реализация интерфейса <see cref="IDisposable"/>.
    /// Освобождает ресурсы, используемые Unit of Work.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        // Подавляем финализацию, так как ресурсы уже освобождены
        GC.SuppressFinalize(this);
    }
}