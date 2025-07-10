using Advisor.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // токен возьмём из переменной окружения
        var token = Environment.GetEnvironmentVariable("TG_TOKEN")
                     ?? throw new InvalidOperationException("TG_TOKEN is not set");

        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        // наши сервисы
        services.AddSingleton<StateService>();
        services.AddSingleton<ChecklistService>();

        // фоновый сервис, который слушает апдейты
        services.AddHostedService<BotBackgroundService>();
    })
    .Build()
    .Run();
