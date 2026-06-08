using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Deadliner.DAL.Repositories
{
    public class UserTokenRepository : BaseRepository<UserToken>, IUserTokenRepository
    {
        public UserTokenRepository(DeadlinerDbContext context) : base(context) { }

        public async Task<UserToken?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}
