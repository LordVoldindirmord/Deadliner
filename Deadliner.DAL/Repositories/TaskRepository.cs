using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Deadliner.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для работы с задачами
    /// </summary>
    public class TaskRepository : BaseRepository<UserTask>, ITaskRepository
    {
        public TaskRepository(DeadlinerDbContext context) : base(context) { }

        public async Task<bool> ExistsByTitleAsync(string title, int tagId)
        {
            return await _dbSet
                .AnyAsync(t => t.TagId == tagId && t.Title == title);
        }

        public async Task<IEnumerable<UserTask>> GetByStatusAsync(int userId, TasksStatus status)
        {
            return await _dbSet
                .Include(t => t.Tag)
                .Where(t => t.Tag.UserId == userId && t.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserTask>> GetByTagIdAsync(int tagId)
        {
            return await _dbSet
                .Where(t => t.TagId == tagId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserTask>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.Tag)
                .Where(t => t.Tag.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserTask>> GetOverdueAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.Tag)
                .Where(t => t.Tag.UserId == userId
                && t.Deadline != null
                && t.Status == TasksStatus.Active
                && t.Deadline < DateTime.Now)
                .ToListAsync();
        }
    }
}
