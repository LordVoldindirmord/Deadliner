using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Keyboards;

internal static class BotKeyboards
{
    public static ReplyKeyboardMarkup Main { get; } = new(new[]
    {
        new KeyboardButton[] { "📊 Дашборд", "📋 Задачи" },
        new KeyboardButton[] { "➕ Задача", "🏷 Теги" },
        new KeyboardButton[] { "ℹ️ Помощь" }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };
}
