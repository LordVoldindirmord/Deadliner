namespace TelegramBot.Handlers;

internal static class BotCommandNormalizer
{
    public static string Normalize(string text)
    {
        return text switch
        {
            "📊 Дашборд" => "/dashboard",
            "📋 Задачи" => "/tasks",
            "➕ Задача" => "/addtask",
            "🏷 Теги" => "/tags",
            "ℹ️ Помощь" => "/help",
            _ => text
        };
    }

    public static string GetCommand(string text)
        => text.Split(' ')[0];
}
