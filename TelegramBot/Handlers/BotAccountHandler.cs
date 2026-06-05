using Deadliner.Service.Interfaces;
using Telegram.Bot;
using TelegramBot.Bot;
using TelegramBot.Keyboards;
using TelegramBot.State;

namespace TelegramBot.Handlers;

/// <summary>
/// Привязка аккаунта, справка и отмена диалога.
/// </summary>
internal sealed class BotAccountHandler
{
    private readonly UserConversationStore _conversations;

    public BotAccountHandler(UserConversationStore conversations)
    {
        _conversations = conversations;
    }

    public bool IsCommand(string command) =>
        command is "/start" or "/help" or "/cancel";

    public bool IsCallback(string data) => data == "help";

    public async Task HandleCommandAsync(BotRequestContext context, string command, string rawText)
    {
        switch (command)
        {
            case "/start":
                await HandleStartAsync(context, rawText);
                break;
            case "/help":
                await HandleHelpAsync(context);
                break;
            case "/cancel":
                _conversations.Remove(context.ChatId);
                await context.Bot.SendMessage(context.ChatId, "❌ Действие отменено.", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
                break;
        }
    }

    public Task HandleCallbackAsync(BotRequestContext context, string data)
        => HandleHelpAsync(context);

    private async Task HandleStartAsync(BotRequestContext context, string rawText)
    {
        var bindingService = context.GetService<ITelegramBindingService>();
        var parts = rawText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length > 1)
        {
            var result = await bindingService.ActivateAsync(parts[1], context.ChatId);

            if (result.IsSuccess)
                await context.Bot.SendMessage(context.ChatId, "✅ Telegram успешно привязан!", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
            else
                await context.Bot.SendMessage(context.ChatId, $"❌ {result.Description}", cancellationToken: context.CancellationToken);
        }
        else
        {
            var userIdResp = await bindingService.GetUserIdByChatIdAsync(context.ChatId);

            if (userIdResp.IsSuccess && userIdResp.Data != null)
                await context.Bot.SendMessage(context.ChatId, "👋 С возвращением!", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
            else
                await context.Bot.SendMessage(context.ChatId, "👋 Добро пожаловать!\n\nПривяжите аккаунт через сайт: Telegram → Привязать Telegram.", cancellationToken: context.CancellationToken);
        }
    }

    private async Task HandleHelpAsync(BotRequestContext context)
    {
        const string helpText =
            """
            📖 **Справка**

            📊 Дашборд — счётчики
            📋 Задачи — список активных задач
            📋 Все задачи — все задачи (любой статус)
            ➕ Задача — добавить новую задачу
            🏷 Теги — управление тегами
            ℹ️ Помощь — эта справка

            Текстовые команды:
            /dashboard, /tasks, /alltasks, /addtask, /tags, /addtag
            /complete [id] — выполнить задачу
            /cancel — отменить текущее действие

            🔗 Привязать аккаунт: сайт → Telegram
            """;

        await context.Bot.SendMessage(context.ChatId, helpText, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
    }
}
