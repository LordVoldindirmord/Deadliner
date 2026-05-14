using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Deadliner.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для работы с пользователями
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DeadlinerDbContext context) : base(context) { }

        public async Task<bool> ExistByEmailAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistByLoginAsync(string login)
        {
            return await _dbSet.AnyAsync(u => u.Login == login);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByLoginAsync(string login)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Login == login);
        }
    }
}
