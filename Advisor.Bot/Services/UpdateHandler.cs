using Advisor.Bot.Handlers;
using Advisor.Bot.State;
using Advisor.Bot.State.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Advisor.Bot.Services;

public sealed class UpdateHandler : IUpdateHandler
{

    private readonly ITelegramBotClient _bot;
    private readonly StateService _states;
    private readonly List<IHandler> _handlers;

    public UpdateHandler(ITelegramBotClient bot, StateService states, IEnumerable<IHandler> handlers)
    {
        _bot = bot;
        _states = states;
        _handlers = handlers.ToList();
    }

    // ✅ новая сигнатура (Bot API v22) - без повторяющихся «_»
    public Task HandleErrorAsync(
     ITelegramBotClient botClient,
     Exception exception,
     HandleErrorSource source,
     CancellationToken ct)
    {
        Console.WriteLine($"TG error ({source}): {exception}");
        return Task.CompletedTask;
    }


    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken ct)
    {
        Console.WriteLine($"[{update.Type}] {update.Message?.Text}");

        if (update.Type != UpdateType.Message) return;

        var state = _states.Get(update.Message!.Chat.Id);
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(update, state));
        if (handler is not null)
            await handler.HandleAsync(botClient, update, state, _states, ct);
    }
}
