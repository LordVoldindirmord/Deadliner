using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.DAL.Repositories
{
    public class TelegramBindingRepository : BaseRepository<TelegramBinding>, ITelegramBindingRepository
    {
        public TelegramBindingRepository(DeadlinerDbContext dbContext) : base(dbContext) { }

        public async Task<TelegramBinding?> GetByChatIdAsync(long chatId)
        {
            return await _dbSet.FirstOrDefaultAsync(binding => binding.TelegramChatId == chatId);
        }

        public async Task<TelegramBinding?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(binding => binding.BindCode == token);
        }

        public async Task<TelegramBinding?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(binding => binding.UserId == userId);
        }
    }
}
