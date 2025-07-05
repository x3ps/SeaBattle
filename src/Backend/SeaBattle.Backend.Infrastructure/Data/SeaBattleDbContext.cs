using Microsoft.EntityFrameworkCore;
using SeaBattle.Backend.Domain.Models; // Убедись, что этот using есть

namespace SeaBattle.Backend.Infrastructure.Data;

/// <summary>
/// Контекст базы данных для приложения "Морской бой",
/// использующий Entity Framework Core.
/// </summary>
public class SeaBattleDbContext : DbContext
{
    /// <summary>
    /// Конструктор, который принимает <see cref="DbContextOptions{TContext}"/>
    /// для конфигурации контекста базы данных.
    /// </summary>
    /// <param name="options">Опции конфигурации для <see cref="SeaBattleDbContext"/>.</param>
    public SeaBattleDbContext(DbContextOptions<SeaBattleDbContext> options) : base(options)
    {
    }

    // DbSet'ы для каждой корневой сущности.
    // Они позволяют EF Core взаимодействовать с соответствующими таблицами в базе данных.
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Ship> Ships { get; set; } = null!;
    public DbSet<BoardHit> BoardHits { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!; // Добавляем DbSet для RefreshToken

    /// <summary>
    /// Этот метод используется для конфигурации модели базы данных с помощью Fluent API.
    /// Здесь определяются первичные ключи, индексы, требования к полям, связи и т.д.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели, используемый для настройки схемы базы данных.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Общая конфигурация для EntityBase (применяется ко всем сущностям, наследующим EntityBase) ---
        // Хотя Id, Created и Modified определены в EntityBase,
        // EF Core автоматически распознает Id как первичный ключ и обрабатывает Created/Modified.
        // Однако, для явной настройки можно было бы использовать:
        // modelBuilder.Entity<EntityBase>().HasKey(e => e.Id);
        // Но так как каждый DbSet относится к конкретной сущности,
        // конфигурация для этих полей часто дублируется или настраивается индивидуально в их Entity-блоках.

        // --- Конфигурация User ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id); // Первичный ключ
            entity.HasIndex(u => u.Name).IsUnique(); // Имя пользователя должно быть уникальным
            entity.Property(u => u.Name).IsRequired().HasMaxLength(50); // Имя обязательно и ограничено по длине
            entity.Property(u => u.PasswordHash).IsRequired(); // Пароль (хеш) обязателен
            entity.Property(u => u.Wins).IsRequired(); // Победы обязательны
            entity.Property(u => u.Losses).IsRequired(); // Поражения обязательны

            // Добавляем связь с RefreshToken: один User может иметь много RefreshToken'ов
            entity.HasMany(u => u.RefreshTokens)
                  .WithOne(rt => rt.User)
                  .HasForeignKey(rt => rt.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade); // При удалении пользователя, удаляем все связанные RefreshToken'ы
        });

        // --- Конфигурация Game ---
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.StartTime).IsRequired();
            entity.Property(g => g.EndTime).IsRequired(false); // EndTime может быть null
            entity.Property(g => g.Status).IsRequired();

            // Отношение: Game имеет одного Player1 (User)
            entity.HasOne(g => g.Player1)
                  .WithMany() // User не имеет коллекции Games, связанной с Player1
                  .HasForeignKey(g => g.Player1Id)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict); // Предотвращает удаление User при удалении Game

            // Отношение: Game имеет одного Player2 (User)
            entity.HasOne(g => g.Player2)
                  .WithMany() // User не имеет коллекции Games, связанной с Player2
                  .HasForeignKey(g => g.Player2Id)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);

            // Отношение: Game может иметь одного Winner (User)
            entity.HasOne(g => g.Winner)
                  .WithMany() // User не имеет коллекции Games, связанной с Winner
                  .HasForeignKey(g => g.WinnerId)
                  .IsRequired(false) // WinnerId может быть null
                  .OnDelete(DeleteBehavior.Restrict); // Предотвращает удаление User при удалении Game
        });

        // --- Конфигурация Board ---
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(b => b.Id);

            // Отношение: Board принадлежит одной Game
            entity.HasOne(b => b.Game)
                  .WithMany() // Game не имеет прямой коллекции Board'ов, связанных с ней
                  .HasForeignKey(b => b.GameId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade); // Удаление Game приводит к удалению Board

            // Отношение: Board принадлежит одному User (владельцу поля)
            entity.HasOne(b => b.User)
                  .WithMany() // User не имеет прямой коллекции Board'ов, связанных с ним
                  .HasForeignKey(b => b.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict); // Предотвращает удаление User при удалении Board
        });

        // --- Конфигурация Ship ---
        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Type).IsRequired();
            entity.Property(s => s.StartX).IsRequired();
            entity.Property(s => s.StartY).IsRequired();
            entity.Property(s => s.Orientation).IsRequired();
            entity.Property(s => s.Hits).IsRequired(); // Новое поле Hits обязательно
            entity.Property(s => s.IsSunk).IsRequired(); // Новое поле IsSunk обязательно
            entity.Ignore(s => s.Size); // Игнорируем вычисляемое свойство Size

            // Отношение: Ship принадлежит одной Board
            entity.HasOne(s => s.Board)
                  .WithMany(b => b.Ships) // Указываем навигационное свойство в Board (ICollection<Ship> Ships)
                  .HasForeignKey(s => s.BoardId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade); // Удаление Board приводит к удалению Ship
        });

        // --- Конфигурация BoardHit ---
        modelBuilder.Entity<BoardHit>(entity =>
        {
            entity.HasKey(bh => bh.Id);
            entity.Property(bh => bh.X).IsRequired();
            entity.Property(bh => bh.Y).IsRequired();
            entity.Property(bh => bh.HitTime).IsRequired();
            // Можно задать значение по умолчанию для HitTime в базе данных
            // entity.Property(bh => bh.HitTime).HasDefaultValueSql("CURRENT_TIMESTAMP"); // Для SQLite/PostgreSQL
            // entity.Property(bh => bh.HitTime).HasDefaultValueSql("GETUTCDATE()"); // Для SQL Server
            entity.Property(bh => bh.IsShipHit).IsRequired();

            // Отношение: BoardHit относится к одной Board
            entity.HasOne(bh => bh.Board)
                  .WithMany(b => b.Hits) // Указываем навигационное свойство в Board (ICollection<BoardHit> Hits)
                  .HasForeignKey(bh => bh.BoardId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade); // Удаление Board приводит к удалению BoardHit

            // Отношение: BoardHit может быть связан с одним Ship (если было попадание)
            entity.HasOne(bh => bh.HitShip)
                  .WithMany() // Ship не имеет прямой коллекции BoardHit'ов, связанных с ним
                  .HasForeignKey(bh => bh.HitShipId)
                  .IsRequired(false) // HitShipId может быть null (промах)
                  .OnDelete(DeleteBehavior.SetNull); // Если Ship удаляется, установить HitShipId в null
        });

        // --- Конфигурация RefreshToken ---
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.Property(rt => rt.Expires).IsRequired();
            entity.Property(rt => rt.Created).IsRequired();
            entity.Property(rt => rt.CreatedByIp).IsRequired().HasMaxLength(45);
            entity.Property(rt => rt.Revoked).IsRequired(false);
            entity.Property(rt => rt.RevokedByIp).IsRequired(false).HasMaxLength(45);
            entity.Property(rt => rt.ReplacedByToken).IsRequired(false).HasMaxLength(256);
            
            entity.Property(rt => rt.ReasonRevoked).IsRequired(false); 

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Переопределяет асинхронный метод сохранения изменений в базу данных.
    /// Используется для автоматической установки полей Created и Modified.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Количество записей, которые были успешно записаны в базу данных.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps(); // Применяем метки времени перед сохранением
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Переопределяет синхронный метод сохранения изменений в базу данных.
    /// Используется для автоматической установки полей Created и Modified.
    /// </summary>
    /// <returns>Количество записей, которые были успешно записаны в базу данных.</returns>
    public override int SaveChanges()
    {
        ApplyTimestamps(); // Применяем метки времени перед сохранением
        return base.SaveChanges();
    }

    /// <summary>
    /// Применяет метки времени (Created и Modified) к отслеживаемым сущностям.
    /// </summary>
    private void ApplyTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is EntityBase && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((EntityBase)entityEntry.Entity).Modified = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((EntityBase)entityEntry.Entity).Created = DateTime.UtcNow;
            }
        }
    }
}