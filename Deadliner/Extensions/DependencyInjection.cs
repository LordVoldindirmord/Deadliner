using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.DAL.Repositories;
using Deadliner.Service.Implementations;
using Deadliner.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Deadliner.ASP.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDeadlinerServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDbContext(services, configuration);
            AddRepositories(services);
            AddServices(services);
            return services;
        }

        private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

            services.AddDbContext<DeadlinerDbContext>(options => options.UseNpgsql(connectionString));
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ITelegramBindingRepository, TelegramBindingRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ITelegramBindingService, TelegramBindingService>();
            services.AddScoped<IEmailService, EmailService>();
        }
    }
}