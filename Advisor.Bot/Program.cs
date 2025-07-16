using Advisor.Bot.Handlers;
using Advisor.Bot.Services;
using Advisor.Bot.State;
using Advisor.Bot.State.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Microsoft.EntityFrameworkCore;
using Advisor.Bot.Data;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Advisor.Bot.Data.Models; // где QuizItem

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
    // независимо от среды
    cfg.AddUserSecrets<Program>(optional: true);
    })
    .ConfigureServices((context, services) =>
    {

        // 1) Прочитать файл и десериализовать
        var json = File.ReadAllText(Path.Combine(context.HostingEnvironment.ContentRootPath, "Content/quiz.json"));
        var map = JsonSerializer.Deserialize<Dictionary<QuizStep, QuizItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? throw new InvalidOperationException("Не удалось загрузить quiz.json");

        // 2) Преобразовать в нужный тип: Dictionary<QuizStep,(string,string[])>
        var typedMap = map.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.Q, kvp.Value.Opts)
        );
        services.AddSingleton(typedMap);


        // context.Configuration уже включает:
        // • appsettings*.json
        // • user-secrets (если среда Development)
        // • переменные окружения
        var cfg = context.Configuration;
        var conn = cfg.GetConnectionString("BotDb");
        //Console.WriteLine($"[DEBUG] TG_TOKEN = {cfg["TG_TOKEN"]}");

        var token = cfg["TG_TOKEN"]                         // User Secrets / appsettings
                 ?? Environment.GetEnvironmentVariable("TG_TOKEN") // env-var
                 ?? throw new InvalidOperationException("TG_TOKEN missing");

        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        services.AddSingleton<StateService>();
        services.AddSingleton<ChecklistService>();

        

        services.AddDbContext<BotDbContext>(opt =>
            opt.UseNpgsql(conn));

        // создаём единственный словарь map и шарим его
        //var map = new Dictionary<QuizStep, (string, string[])>(QuizHandler.DefaultMap);
        //services.AddSingleton(map);

        services.AddSingleton<IHandler, StartCommandHandler>();
        services.AddSingleton<IHandler, CallbackHandler>();
        services.AddSingleton<IHandler, QuizHandler>();

        services.AddSingleton<IUpdateHandler, UpdateHandler>();
        services.AddHostedService<BotBackgroundService>();
    })
    .Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
        db.Database.Migrate();
    }

await host.RunAsync();
