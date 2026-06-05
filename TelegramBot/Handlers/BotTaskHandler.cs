using Deadliner.Domain.Enum;
using Deadliner.Domain.ViewModel.Tasks;
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
/// Дашборд, списки задач, выполнение и создание задач.
/// </summary>
internal sealed class BotTaskHandler
{
    private readonly UserConversationStore _conversations;

    public BotTaskHandler(UserConversationStore conversations)
    {
        _conversations = conversations;
    }

    public bool IsCommand(string command) =>
        command is "/dashboard" or "/tasks" or "/alltasks" or "/complete" or "/addtask";

    public bool IsConversation(string state) => state.StartsWith("adding_task:");

    public bool IsCallback(string data) =>
        data is "dashboard" or "tasks" or "alltasks"
        || data.StartsWith("complete:")
        || data.StartsWith("addtask:");

    public async Task HandleCommandAsync(BotRequestContext context, string command, string rawText)
    {
        switch (command)
        {
            case "/dashboard":
                await ShowDashboardAsync(context);
                break;
            case "/tasks":
                await ShowActiveTasksAsync(context);
                break;
            case "/alltasks":
                await ShowAllTasksAsync(context);
                break;
            case "/complete":
                await CompleteByIdAsync(context, rawText);
                break;
            case "/addtask":
                await ShowTagPickerAsync(context);
                break;
        }
    }

    public async Task HandleConversationAsync(BotRequestContext context, string state, string title)
    {
        var tagId = int.Parse(state.Split(':')[1]);
        _conversations.Remove(context.ChatId);

        var taskService = context.GetService<ITaskService>();
        var model = new TaskCreateViewModel
        {
            Title = title,
            TagId = tagId,
            Priority = PriorityStatus.Medium
        };

        var response = await taskService.CreateAsync(context.UserId!.Value, model);

        if (response.IsSuccess)
            await context.Bot.SendMessage(context.ChatId, $"✅ Задача «{title}» создана!", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
        else
            await context.Bot.SendMessage(context.ChatId, $"❌ {response.Description}", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
    }

    public async Task HandleCallbackAsync(BotRequestContext context, string data)
    {
        switch (data)
        {
            case "dashboard":
                await ShowDashboardAsync(context);
                break;
            case "tasks":
                await ShowActiveTasksAsync(context);
                break;
            case "alltasks":
                await ShowAllTasksAsync(context);
                break;
            default:
                if (data.StartsWith("complete:") && int.TryParse(data.Split(':')[1], out int index))
                    await CompleteByIndexAsync(context, index);
                else if (data.StartsWith("addtask:") && int.TryParse(data.Split(':')[1], out int tagId))
                    await StartAddingTaskAsync(context, tagId);
                break;
        }
    }

    private async Task ShowDashboardAsync(BotRequestContext context)
    {
        var taskService = context.GetService<ITaskService>();
        var response = await taskService.GetDashboardAsync(context.UserId!.Value, 0);

        if (!response.IsSuccess || response.Data == null)
        {
            await context.Bot.SendMessage(context.ChatId, $"❌ {response.Description ?? "Ошибка"}", cancellationToken: context.CancellationToken);
            return;
        }

        var d = response.Data;
        var sb = new StringBuilder();
        sb.AppendLine("📊 **Дашборд**\n");
        sb.AppendLine($"🟢 Активные: {d.ActiveCount}");
        sb.AppendLine($"🔴 Просрочено: {d.OverdueCount}");
        sb.AppendLine($"✅ Выполнено: {d.CompletedCount}");
        sb.AppendLine($"🚫 Отменено: {d.CancelledCount}");

        await context.Bot.SendMessage(context.ChatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
    }

    private async Task ShowActiveTasksAsync(BotRequestContext context)
    {
        var taskList = await GetActiveTasksAsync(context);

        if (!taskList.Any())
        {
            await context.Bot.SendMessage(context.ChatId, "📭 Нет активных задач.", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("📋 **Активные задачи:**\n");

        var taskButtons = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < taskList.Count; i++)
        {
            var item = taskList[i];
            var number = i + 1;
            var deadline = item.Deadline != null ? $" — {item.Deadline:dd.MM}" : "";
            sb.AppendLine($"{number}. {GetPriorityIcon(item.Priority)} {item.Title}{deadline}");
            taskButtons.Add(new[] { InlineKeyboardButton.WithCallbackData($"✅ Выполнить «{number}»", $"complete:{number}") });
        }

        taskButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("📋 Все задачи (все статусы)", "alltasks") });

        await context.Bot.SendMessage(context.ChatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(taskButtons), cancellationToken: context.CancellationToken);
    }

    private async Task ShowAllTasksAsync(BotRequestContext context)
    {
        var taskService = context.GetService<ITaskService>();
        var filter = new TaskFilterViewModel { SortBy = "deadline", SortDirection = "asc" };
        var response = await taskService.GetFilteredAsync(context.UserId!.Value, filter);

        if (!response.IsSuccess || response.Data == null || !response.Data.Tasks.Any())
        {
            await context.Bot.SendMessage(context.ChatId, "📭 У вас нет задач.", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
            return;
        }

        var tasks = response.Data.Tasks.Take(20).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("📋 **Все задачи:**\n");

        var taskButtons = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            var number = i + 1;

            var statusIcon = task.Status switch
            {
                TasksStatus.Active => task.IsOverdue ? "🔴" : "🟢",
                TasksStatus.Completed => "✅",
                TasksStatus.Cancelled => "🚫",
                _ => "⚪"
            };

            var priority = task.Priority switch
            {
                PriorityStatus.Critical => "[!]",
                PriorityStatus.High => "[↑]",
                PriorityStatus.Medium => "[~]",
                PriorityStatus.Low => "[↓]",
                _ => ""
            };

            var deadline = task.Deadline != null ? $" — {task.Deadline:dd.MM}" : "";
            var statusText = task.Status switch
            {
                TasksStatus.Active => task.IsOverdue ? "ПРОСРОЧЕНА" : "активна",
                TasksStatus.Completed => "выполнена",
                TasksStatus.Cancelled => "отменена",
                _ => ""
            };

            sb.AppendLine($"{number}. {statusIcon} {priority} {task.Title}{deadline}");
            sb.AppendLine($"   _{task.Tag?.Name ?? "—"} | {statusText}_");

            if (task.Status == TasksStatus.Active)
                taskButtons.Add(new[] { InlineKeyboardButton.WithCallbackData($"✅ Выполнить «{number}»", $"complete:{number}") });
        }

        await context.Bot.SendMessage(context.ChatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(taskButtons), cancellationToken: context.CancellationToken);
    }

    private async Task CompleteByIdAsync(BotRequestContext context, string rawText)
    {
        var parts = rawText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || !int.TryParse(parts[1], out int taskId))
        {
            await context.Bot.SendMessage(context.ChatId, "❌ Укажите ID задачи: /complete 5\n\nID можно узнать в /tasks.", cancellationToken: context.CancellationToken);
            return;
        }

        var taskService = context.GetService<ITaskService>();
        var response = await taskService.CompleteAsync(context.UserId!.Value, taskId);

        if (response.IsSuccess)
            await context.Bot.SendMessage(context.ChatId, "✅ Задача выполнена!", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
        else
            await context.Bot.SendMessage(context.ChatId, $"❌ {response.Description}", cancellationToken: context.CancellationToken);
    }

    private async Task CompleteByIndexAsync(BotRequestContext context, int taskIndex)
    {
        var taskList = await GetActiveTasksAsync(context);

        if (!taskList.Any())
        {
            await context.Bot.SendMessage(context.ChatId, "📭 Нет активных задач.", cancellationToken: context.CancellationToken);
            return;
        }

        if (taskIndex < 1 || taskIndex > taskList.Count)
        {
            await context.Bot.SendMessage(context.ChatId, "❌ Неверный номер.", cancellationToken: context.CancellationToken);
            return;
        }

        var taskItem = taskList[taskIndex - 1];
        var taskService = context.GetService<ITaskService>();
        var response = await taskService.CompleteAsync(context.UserId!.Value, taskItem.Id);

        if (response.IsSuccess)
            await context.Bot.SendMessage(context.ChatId, $"✅ Задача «{taskItem.Title}» выполнена!", replyMarkup: BotKeyboards.Main, cancellationToken: context.CancellationToken);
        else
            await context.Bot.SendMessage(context.ChatId, $"❌ {response.Description}", cancellationToken: context.CancellationToken);
    }

    private async Task ShowTagPickerAsync(BotRequestContext context)
    {
        var tagService = context.GetService<ITagService>();
        var response = await tagService.GetByUserIdAsync(context.UserId!.Value);

        if (!response.IsSuccess || response.Data == null || !response.Data.Any())
        {
            var emptyButtons = new List<InlineKeyboardButton[]>
            {
                new[] { InlineKeyboardButton.WithCallbackData("➕ Создать тег", "addtag") }
            };

            await context.Bot.SendMessage(context.ChatId, "Сначала создайте тег.", replyMarkup: new InlineKeyboardMarkup(emptyButtons), cancellationToken: context.CancellationToken);
            return;
        }

        var tagButtons = response.Data
            .Select(tag => new[] { InlineKeyboardButton.WithCallbackData(tag.Name, $"addtask:{tag.Id}") })
            .ToList();

        await context.Bot.SendMessage(context.ChatId, "Выберите тег для новой задачи:\n", replyMarkup: new InlineKeyboardMarkup(tagButtons), cancellationToken: context.CancellationToken);
    }

    private async Task StartAddingTaskAsync(BotRequestContext context, int tagId)
    {
        _conversations.Set(context.ChatId, $"adding_task:{tagId}");
        await context.Bot.SendMessage(context.ChatId, "📝 Введите название задачи:\n\n_Для отмены: /cancel_", parseMode: ParseMode.Markdown, cancellationToken: context.CancellationToken);
    }

    private async Task<IReadOnlyList<TaskDetailViewModel>> GetActiveTasksAsync(BotRequestContext context)
    {
        var taskService = context.GetService<ITaskService>();
        var filter = new TaskFilterViewModel
        {
            Status = TasksStatus.Active,
            SortBy = "deadline",
            SortDirection = "asc"
        };

        var response = await taskService.GetFilteredAsync(context.UserId!.Value, filter);

        if (!response.IsSuccess || response.Data == null)
            return Array.Empty<TaskDetailViewModel>();

        return response.Data.Tasks.Take(10).ToList();
    }

    private static string GetPriorityIcon(PriorityStatus priority) => priority switch
    {
        PriorityStatus.Critical => "🔴",
        PriorityStatus.High => "🟠",
        PriorityStatus.Medium => "🟡",
        PriorityStatus.Low => "🟢",
        _ => "⚪"
    };
}
