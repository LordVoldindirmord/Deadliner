using Microsoft.Extensions.DependencyInjection;
using TelegramBot;
using TelegramBot.Bot;
using TelegramBot.Settings;

string jsonFilePath = "appsettings.json";
AppSettings settings = SettingsLoader.GetSettings(jsonFilePath);

IServiceCollection services = new ServiceCollection();
services.AddBotServices(settings);

var serviceProvider = services.BuildServiceProvider();

var botRunner = serviceProvider.GetRequiredService<BotRunner>();
await botRunner.RunAsync();
