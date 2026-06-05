using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Deadliner.DAL.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(DeadlinerDbContext context) : base(context) { }

        public async Task<bool> ExistsByNameAsync(string name, int userId)
        {
            return await _dbSet.AnyAsync(t => t.UserId == userId && t.Name == name);
        }

        public async Task<IEnumerable<Tag>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<(Tag tag, int taskCount)>> GetByUserIdWithTaskCountAsync(int userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .GroupJoin(
                _context.UserTasks,
                tag => tag.Id,
                task => task.TagId,
                (tag, tasks) => new
                {
                    tag,
                    count = tasks.Count(),
                })
                .Select(x => ValueTuple.Create(x.tag, x.count))
                .ToListAsync();
        }
    }
}
