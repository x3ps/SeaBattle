using SeaBattle.Backend.Domain.Models;

namespace SeaBattle.Backend.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByNameAsync(string name);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
}