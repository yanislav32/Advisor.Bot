using System.Threading;
using System.Threading.Tasks;
using Advisor.Bot.State;
using Advisor.Bot.State.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Advisor.Bot.Handlers;

/// <summary>Базовый интерфейс любого обработчика апдейтов.</summary>
public interface IHandler
{
    bool CanHandle(Update update, UserState state);

    Task HandleAsync(
        ITelegramBotClient bot,
        Update update,
        UserState state,
        StateService states,
        CancellationToken ct);
}
