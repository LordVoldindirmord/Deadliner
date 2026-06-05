using Deadliner.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.State;

namespace TelegramBot.Bot;

internal sealed class UpdateRouter
{
    private const string UnboundMessage = "❌ Вы не привязаны к аккаунту.\n\nПривяжите Telegram на сайте: раздел «Telegram» → «Привязать Telegram».";

    private readonly IServiceProvider _serviceProvider;
    private readonly UserConversationStore _conversations;
    private readonly Handlers.BotAccountHandler _account;
    private readonly Handlers.BotTaskHandler _tasks;
    private readonly Handlers.BotTagHandler _tags;

    public UpdateRouter(
        IServiceProvider serviceProvider,
        UserConversationStore conversations,
        Handlers.BotAccountHandler account,
        Handlers.BotTaskHandler tasks,
        Handlers.BotTagHandler tags)
    {
        _serviceProvider = serviceProvider;
        _conversations = conversations;
        _account = account;
        _tasks = tasks;
        _tags = tags;
    }

    public async Task RouteAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { } query)
        {
            await RouteCallbackAsync(bot, query, cancellationToken);
            return;
        }

        if (update.Message?.Text is { } messageText)
            await RouteMessageAsync(bot, update.Message, messageText, cancellationToken);
    }

    private async Task RouteMessageAsync(ITelegramBotClient bot, Message message, string text, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var originalText = text;

        using var scope = _serviceProvider.CreateScope();
        var userId = await ResolveUserIdAsync(scope, chatId);
        var context = CreateContext(scope, bot, chatId, userId, cancellationToken);

        var normalizedText = Handlers.BotCommandNormalizer.Normalize(text);
        var command = Handlers.BotCommandNormalizer.GetCommand(normalizedText);

        if (command == "/start")
        {
            _conversations.Remove(chatId);
            await _account.HandleCommandAsync(context, command, normalizedText);
            return;
        }

        if (userId == null)
        {
            await bot.SendMessage(chatId, UnboundMessage, cancellationToken: cancellationToken);
            return;
        }

        if (TryGetCommandHandler(command, out var commandHandler))
        {
            if (_conversations.TryGet(chatId, out var state) && state != null)
                _conversations.Remove(chatId);

            await commandHandler(context, command, normalizedText);
            return;
        }

        if (_conversations.TryGet(chatId, out var activeState) && activeState != null)
        {
            if (_tasks.IsConversation(activeState))
            {
                await _tasks.HandleConversationAsync(context, activeState, originalText);
                return;
            }

            if (_tags.IsConversation(activeState))
            {
                await _tags.HandleConversationAsync(context, activeState, originalText);
                return;
            }
        }

        await bot.SendMessage(chatId, "Неизвестная команда. Напишите /help для списка команд.", cancellationToken: cancellationToken);
    }

    private async Task RouteCallbackAsync(ITelegramBotClient bot, CallbackQuery query, CancellationToken cancellationToken)
    {
        await bot.AnswerCallbackQuery(query.Id, cancellationToken: cancellationToken);

        var chatId = query.Message!.Chat.Id;
        var callbackData = query.Data ?? "";

        using var scope = _serviceProvider.CreateScope();
        var userId = await ResolveUserIdAsync(scope, chatId);

        if (userId == null)
        {
            await bot.SendMessage(chatId, "❌ Вы не привязаны к аккаунту.", cancellationToken: cancellationToken);
            return;
        }

        if (_conversations.TryGet(chatId, out var state)
            && state != null
            && !callbackData.StartsWith("addtask:"))
        {
            _conversations.Remove(chatId);
        }

        var context = CreateContext(scope, bot, chatId, userId, cancellationToken);

        if (_account.IsCallback(callbackData))
            await _account.HandleCallbackAsync(context, callbackData);
        else if (_tasks.IsCallback(callbackData))
            await _tasks.HandleCallbackAsync(context, callbackData);
        else if (_tags.IsCallback(callbackData))
            await _tags.HandleCallbackAsync(context, callbackData);
    }

    private bool TryGetCommandHandler(string command, out Func<BotRequestContext, string, string, Task> handler)
    {
        if (_account.IsCommand(command))
        {
            handler = _account.HandleCommandAsync;
            return true;
        }

        if (_tasks.IsCommand(command))
        {
            handler = _tasks.HandleCommandAsync;
            return true;
        }

        if (_tags.IsCommand(command))
        {
            handler = _tags.HandleCommandAsync;
            return true;
        }

        handler = null!;
        return false;
    }

    private static BotRequestContext CreateContext(
        IServiceScope scope,
        ITelegramBotClient bot,
        long chatId,
        int? userId,
        CancellationToken cancellationToken)
    {
        return new BotRequestContext
        {
            ChatId = chatId,
            UserId = userId,
            Scope = scope,
            Bot = bot,
            CancellationToken = cancellationToken
        };
    }

    private static async Task<int?> ResolveUserIdAsync(IServiceScope scope, long chatId)
    {
        var bindingService = scope.ServiceProvider.GetRequiredService<ITelegramBindingService>();
        var response = await bindingService.GetUserIdByChatIdAsync(chatId);
        return response.IsSuccess ? response.Data : null;
    }
}
