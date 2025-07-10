using Advisor.Bot.State;
using Advisor.Bot.State.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;



namespace Advisor.Bot.Handlers;

internal sealed class StartCommandHandler : IHandler
{
    public bool CanHandle(Update u, UserState _) =>
        u.Message?.Text is "/start" or "Начать тест";

    public async Task HandleAsync(ITelegramBotClient bot, Update u,
        UserState state, StateService states, CancellationToken ct)
    {
        long chat = u.Message!.Chat.Id;
        states.Reset(chat);                 // чистый лист
        state = states.Get(chat);
        state.Step = QuizStep.Role;         // первый вопрос

        await bot.SendMessage(chat,
            "🔸 Пройдём короткий финансовый чек-ап. Отвечай честно 😉",
            replyMarkup: BuildReplyKeyboard("Поехали"),
            cancellationToken: ct);
    }

    private static ReplyKeyboardMarkup BuildReplyKeyboard(params string[] buttons) =>
        new(buttons.Select(b => new[] { new KeyboardButton(b) }))
        { ResizeKeyboard = true, OneTimeKeyboard = true };
}
