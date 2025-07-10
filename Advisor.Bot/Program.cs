using Advisor.Bot.Handlers;
using Advisor.Bot.Services;
using Advisor.Bot.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // context.Configuration уже включает:
        // • appsettings*.json
        // • user-secrets (если среда Development)
        // • переменные окружения
        var cfg = context.Configuration;
        var token = cfg["TG_TOKEN"]                         // User Secrets / appsettings
                 ?? Environment.GetEnvironmentVariable("TG_TOKEN") // env-var
                 ?? throw new InvalidOperationException("TG_TOKEN missing");

        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        services.AddSingleton<StateService>();
        services.AddSingleton<ChecklistService>();

        services.AddSingleton<IHandler, StartCommandHandler>();
        services.AddSingleton<IHandler, QuizHandler>();

        services.AddSingleton<IUpdateHandler, UpdateHandler>();
        services.AddHostedService<BotBackgroundService>();
    })
    .Build();

await host.RunAsync();
