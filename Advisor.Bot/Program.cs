using Advisor.Bot.Handlers;
using Advisor.Bot.Services;
using Advisor.Bot.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        var token = Environment.GetEnvironmentVariable("TG_TOKEN")
                     ?? throw new InvalidOperationException("TG_TOKEN missing");

        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        // DI наших классов
        services.AddSingleton<StateService>();
        services.AddSingleton<ChecklistService>();

        // регистрируем обработчики
        services.AddSingleton<IHandler, StartCommandHandler>();
        services.AddSingleton<IHandler, QuizHandler>();

        // UpdateHandler зависит от StateService и списка IHandler
        services.AddSingleton<IUpdateHandler, UpdateHandler>();

        services.AddHostedService<BotBackgroundService>();
    })
    .Build();

await host.RunAsync();
