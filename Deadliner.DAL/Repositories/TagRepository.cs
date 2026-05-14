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

        //public async Task Test()
        //{
        //    var tags = _dbSet;
        //    var users = _context.Users;

        //    var result = users
        //        .GroupJoin(
        //        tags,
        //        u => u.Id,
        //        t => t.UserId,
        //        (user, tag) => new
        //        {
        //            UserId = user.Id,
        //            UserLogin = user.Login,
        //            UserEmail = user.Email,
        //            Tags = tag,
        //        });

        //    foreach(var item in result)
        //    {
        //        Console.WriteLine($"({item.UserId}): {item.UserLogin}");

        //        foreach (var item2 in item.Tags)
        //            Console.WriteLine($"\t\t\t({item2.Id}): {item2.Name} - ({item2.UserId})");
        //    }
        //}
    }
}
