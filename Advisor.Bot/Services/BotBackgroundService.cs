using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Advisor.Bot.Services;

public sealed class BotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUpdateHandler _handler;

    public BotBackgroundService(ITelegramBotClient bot, IUpdateHandler handler)
    {
        _bot = bot;
        _handler = handler;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(_handler, _handler,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: stoppingToken);
        return Task.CompletedTask;
    }
}
