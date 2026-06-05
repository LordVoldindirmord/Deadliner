namespace TelegramBot.Settings
{
    internal class AppSettings
    {
        public ConnectionStringSettings ConnectionString { get; set; }
        public TelegramBotSettings TelegramBot { get; set; }
    }

    internal class ConnectionStringSettings
    {
        public string DefaultConnection { get; set; }
    }

    internal class TelegramBotSettings
    {
        public string Token { get; set; }
        public string UserName { get; set; }
    }
}
