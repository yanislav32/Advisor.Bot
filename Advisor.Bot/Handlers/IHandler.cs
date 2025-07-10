using System.Threading;
using System.Threading.Tasks;
using Advisor.Bot.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Advisor.Bot.Handlers;

internal interface IHandler
{
    bool CanHandle(Update update, UserState state);
    Task HandleAsync(ITelegramBotClient bot, Update update, UserState state,
                     StateService states, CancellationToken ct);
}
