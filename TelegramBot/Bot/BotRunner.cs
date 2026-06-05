using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Bot;

internal sealed class BotRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly UpdateRouter _updateRouter;
    private readonly CancellationTokenSource _cts = new();

    public BotRunner(IServiceProvider serviceProvider, UpdateRouter updateRouter)
    {
        _serviceProvider = serviceProvider;
        _updateRouter = updateRouter;
    }

    public async Task RunAsync()
    {
        var bot = _serviceProvider.GetRequiredService<TelegramBotClient>();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
        };

        bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token);

        var me = await bot.GetMe();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Бот @{me.Username} запущен. Нажмите Enter для выхода...");
        Console.ResetColor();
        Console.ReadLine();

        _cts.Cancel();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Бот остановлен");
        Console.ResetColor();
    }

    private Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        => _updateRouter.RouteAsync(client, update, token);

    private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка: {exception.Message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}
