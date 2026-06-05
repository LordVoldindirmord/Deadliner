using System.Text.Json;

namespace TelegramBot.Settings
{
    internal static class SettingsLoader
    {
        public static AppSettings GetSettings(string jsonFilePath)
        {
            return Serialization(jsonFilePath);
        }

        private static string GetJsonString(string jsonFilePath) => File.ReadAllText(jsonFilePath);

        private static AppSettings Serialization(string jsonFilePath)
        {
            string jsonString = GetJsonString(jsonFilePath);

            AppSettings? appSettings;

            try
            {
                appSettings = JsonSerializer.Deserialize<AppSettings>(jsonString);
            }
            catch (JsonException ex)
            {
                throw new FormatException("Ошибка формата JSON файла", ex);
            }

            if (appSettings == null)
                throw new InvalidOperationException("Не удалось загрузить настройки");

            if (appSettings.TelegramBot?.Token == null)
                throw new ArgumentNullException(nameof(appSettings.TelegramBot.Token), "Не удалось получить токен Telegram бота");

            if (appSettings.TelegramBot?.UserName == null)
                throw new ArgumentNullException(nameof(appSettings.TelegramBot.UserName), "Не удалось получить имя бота");

            if (appSettings.ConnectionString?.DefaultConnection == null)
                throw new ArgumentNullException(nameof(appSettings.ConnectionString.DefaultConnection), "Не удалось получить строку подключения к базе данных");

            return appSettings;
        }
    }
}
