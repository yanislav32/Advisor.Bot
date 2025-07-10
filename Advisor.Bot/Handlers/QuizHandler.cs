using Advisor.Bot.State;
using Advisor.Bot.State.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Advisor.Bot.Handlers;

internal sealed class QuizHandler : IHandler
{
    // ❶ Карта «шаг → (вопрос, кнопки)»
    private readonly Dictionary<QuizStep, (string Q, string[] Opts)> _map = new()
    {
        [QuizStep.Role] = ("Кто вы по профессии?", new[] { "Сотрудник", "Фрилансер", "Предприниматель" }),
        [QuizStep.Experience] = ("Сколько лет стажа?", new[] { "До 3", "3-10", "10+" }),
        [QuizStep.Capital] = ("Какой объём капитала (₽)?", new[] { "0", "До 1 млн", "Больше" }),
        [QuizStep.IncomeSources] = ("Сколько источников дохода?", new[] { "1", "2-3", "4+" }),
        [QuizStep.SpareMoney] = ("Остаётся ли >10 % после трат?", new[] { "Да", "Нет" }),
        [QuizStep.ExpenseTracking] = ("Ведёте учёт расходов?", new[] { "Всегда", "Иногда", "Нет" }),
        [QuizStep.BudgetLeak] = ("Знаете «дыры» бюджета?", new[] { "Да", "Нет" }),
        [QuizStep.Reserve] = ("Есть резерв 3-6 мес.?", new[] { "Да", "Нет" }),
        [QuizStep.Goal] = ("Назовите финансовую цель", Array.Empty<string>()),
    };

    public bool CanHandle(Update u, UserState s)
    {
        // ловим только текстовые Messages
        return u.Message is { Type: Telegram.Bot.Types.Enums.MessageType.Text } m
               && s.Step is >= QuizStep.Role and < QuizStep.Finished;
    }

    public async Task HandleAsync(ITelegramBotClient bot, Update u,
        UserState state, StateService states, CancellationToken ct)
    {
        long chat = u.Message!.Chat.Id;
        var answer = u.Message.Text!.Trim();

        // ❷ Сохраняем ответ к текущему шагу
        state.Answers[state.Step] = answer;

        // ❸ Вычисляем следующий шаг
        state.Step = Next(state.Step);

        if (state.Step == QuizStep.Finished)
        {
            // конец теста: отдаём чек-лист
            var checklist = new ChecklistService().Build(state.Answers); // позже DI
            await bot.SendTextMessageAsync(chat, checklist, cancellationToken: ct);
            states.Reset(chat);
            return;
        }

        // ❹ Шлём следующий вопрос
        var (q, opts) = _map[state.Step];
        await bot.SendTextMessageAsync(chat, q,
            replyMarkup: BuildReply(opts), cancellationToken: ct);
    }

    private static QuizStep Next(QuizStep step) =>
        step == QuizStep.Goal ? QuizStep.Finished : (QuizStep)((int)step + 1);

    private static ReplyKeyboardMarkup BuildReply(string[] opts) =>
        opts.Length == 0
            ? new ReplyKeyboardRemove()
            : new(opts.Select(o => new[] { new KeyboardButton(o) }))
            { ResizeKeyboard = true, OneTimeKeyboard = true };
}
