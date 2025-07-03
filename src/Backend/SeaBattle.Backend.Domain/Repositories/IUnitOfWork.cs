namespace SeaBattle.Backend.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Свойства для доступа к конкретным репозиториям
    IUserRepository Users { get; }
    IGameRepository Games { get; }

    /// <summary>
    /// Сохраняет все изменения, отслеживаемые в текущей единице работы, в базу данных.
    /// </summary>
    /// <returns>Количество измененных записей в базе данных.</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Откатывает все изменения, отслеживаемые в текущей единице работы.
    /// (В EF Core это обычно достигается через IDisposable или сброс DbContext,
    /// но явный метод может быть полезен в некоторых сценариях).
    /// </summary>
    void Rollback(); // Добавляем метод для отката, если это необходимо
}