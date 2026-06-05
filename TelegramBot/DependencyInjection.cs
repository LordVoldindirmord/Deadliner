using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.DAL.Repositories;
using Deadliner.Service.Implementations;
using Deadliner.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBot.Bot;
using TelegramBot.Handlers;
using TelegramBot.Settings;
using TelegramBot.State;

namespace TelegramBot;

internal static class DependencyInjection
{
    public static IServiceCollection AddBotServices(this IServiceCollection services, AppSettings settings)
    {
        AddDatabase(services, settings);
        AddRepositories(services);
        AddServices(services);
        AddTelegramClient(services, settings);
        AddBotInfrastructure(services);
        return services;
    }

    private static void AddDatabase(IServiceCollection services, AppSettings settings)
    {
        services.AddDbContext<DeadlinerDbContext>(options => options
            .UseNpgsql(settings.ConnectionString.DefaultConnection)
            .LogTo(Console.WriteLine));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITelegramBindingRepository, TelegramBindingRepository>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITelegramBindingService, TelegramBindingService>();
    }

    private static void AddTelegramClient(IServiceCollection services, AppSettings settings)
    {
        services.AddSingleton(settings.TelegramBot);
        services.AddSingleton(new TelegramBotClient(settings.TelegramBot.Token));
    }

    private static void AddBotInfrastructure(IServiceCollection services)
    {
        services.AddSingleton<UserConversationStore>();
        services.AddSingleton<BotAccountHandler>();
        services.AddSingleton<BotTaskHandler>();
        services.AddSingleton<BotTagHandler>();
        services.AddSingleton<UpdateRouter>();
        services.AddSingleton<BotRunner>();
    }
}
