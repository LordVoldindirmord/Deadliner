using Deadliner.Service.Interfaces;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Bot;
using TelegramBot.Keyboards;
using TelegramBot.State;

namespace TelegramBot.Handlers;

/// <summary>
/// Список тегов, создание и удаление.
/// </summary>
internal sealed class BotTagHandler
{
    private readonly UserConversationStore _conversations;

    public BotTagHandler(UserConversationStore conversations)
    {
        _conversations = conversations;
    }

    public bool IsCommand(string command) => command is "/tags" or "/addtag";

    public bool IsConversation(string state) => state == "adding_tag";

    public bool IsCallback(string data) =>
        data == "addtag" || data.StartsWith("deletetag:");

    public async Task HandleCommandAsync(BotRequestContext context, string command, string rawText)
    {
        switch (command)
        {
            case "/tags":
                await ShowTagsAsync(context);
                break;
            case "/addtag":
                await StartAddingTagAsync(context);
                break;
        }
    }

    public async Task HandleConversationAsync(BotRequestContext context, string state, string tagName)
    {
        _conversations.Remove(context.ChatId);

        var tagService = context.GetService<ITagService>();
        var model = new Deadliner.Domain.ViewModel.Tags.TagCreateViewModel
        {
            Name = tagName,
            ColorHex = "#" + Random.Shared.Next(0x100000, 0xFFFFFF).ToString("X6")
        };

        var response = await tagService.CreateAsync(context.UserId!.Value, model);

        if (response.IsSuccess)
            await context.Bot.SendMessage(context.ChatId, $"✅ Тег «{tagName}» создан!", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
        else
            await context.Bot.SendMessage(context.ChatId, $"❌ {response.Description}", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
    }

    public async Task HandleCallbackAsync(BotRequestContext context, string data)
    {
        if (data == "addtag")
        {
            await StartAddingTagAsync(context);
            return;
        }

        if (data.StartsWith("deletetag:") && int.TryParse(data.Split(':')[1], out int tagId))
            await DeleteTagAsync(context, tagId);
    }

    private async Task ShowTagsAsync(BotRequestContext context)
    {
        var tagService = context.GetService<ITagService>();
        var response = await tagService.GetByUserIdAsync(context.UserId!.Value);

        if (!response.IsSuccess || response.Data == null || !response.Data.Any())
        {
            var emptyButtons = new List<InlineKeyboardButton[]>
            {
                new[] { InlineKeyboardButton.WithCallbackData("➕ Создать тег", "addtag") }
            };

            await context.Bot.SendMessage(context.ChatId, "📭 У вас нет тегов.", replyMarkup: new InlineKeyboardMarkup(emptyButtons), cancellationToken: context.CancellationToken);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("🏷 **Ваши теги:**\n");

        var tagButtons = new List<InlineKeyboardButton[]>();

        foreach (var tag in response.Data)
        {
            sb.AppendLine($"• {tag.Name} ({tag.ActiveTaskCount} задач)");
            tagButtons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData($"➕ Задача в «{tag.Name}»", $"addtask:{tag.Id}"),
                InlineKeyboardButton.WithCallbackData("🗑 Удалить", $"deletetag:{tag.Id}")
            });
        }

        tagButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("🏷 Создать новый тег", "addtag") });

        await context.Bot.SendMessage(context.ChatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(tagButtons), cancellationToken: context.CancellationToken);
    }

    private async Task StartAddingTagAsync(BotRequestContext context)
    {
        _conversations.Set(context.ChatId, "adding_tag");
        await context.Bot.SendMessage(context.ChatId, "🏷 Введите название нового тега:\n\n_Для отмены: /cancel_", parseMode: ParseMode.Markdown, cancellationToken: context.CancellationToken);
    }

    private async Task DeleteTagAsync(BotRequestContext context, int tagId)
    {
        var tagService = context.GetService<ITagService>();
        var response = await tagService.DeleteAsync(context.UserId!.Value, tagId);

        if (response.IsSuccess)
            await context.Bot.SendMessage(context.ChatId, "✅ Тег удалён.", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
        else
            await context.Bot.SendMessage(context.ChatId, $"❌ {response.Description}", cancellationToken: context.CancellationToken);
    }
}
