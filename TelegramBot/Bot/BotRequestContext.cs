using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace TelegramBot.Bot;

internal sealed class BotRequestContext
{
    public required long ChatId { get; init; }
    public int? UserId { get; init; }
    public required IServiceScope Scope { get; init; }
    public required ITelegramBotClient Bot { get; init; }
    public CancellationToken CancellationToken { get; init; }

    public T GetService<T>() where T : notnull
        => Scope.ServiceProvider.GetRequiredService<T>();
}
