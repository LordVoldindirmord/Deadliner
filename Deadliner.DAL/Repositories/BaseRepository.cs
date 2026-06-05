using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Deadliner.DAL.Repositories
{
    /// <summary>
    /// Базовый класс для всех репозиториев
    /// </summary>
    /// <typeparam name="T">Класс модели, с которой работает конкретный репозиторий</typeparam>
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly DbSet<T> _dbSet;
        protected readonly DeadlinerDbContext _context;

        protected BaseRepository(DeadlinerDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
