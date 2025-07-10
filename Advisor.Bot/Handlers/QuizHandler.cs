using Advisor.Bot.State;
using Advisor.Bot.State.Models;
using Advisor.Bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;





namespace Advisor.Bot.Handlers;

internal sealed class QuizHandler : IHandler
{
    // ❶ Карта «шаг → (вопрос, кнопки)»
    private readonly Dictionary<QuizStep,(string Q,string[] Opts)> _map = new()
    {
        [QuizStep.Role]          = ("Каĸ бы вы описали свою роль?",                     new[] { "Предприниматель", "Руководитель", "Специалист", "Другое" }),
        [QuizStep.Experience]    = ("Опыт инвестиций?",                                 new[] { "Начинающий", "1–3 года", "3+ лет" }),
        [QuizStep.Capital]       = ("Свободный ĸапитал, ĸоторым готовы управлять?",     new[] { " 1 млн ₽", " 1-5 млн ₽", " 5+ млн ₽" }),
        [QuizStep.IncomeSources] = ("Сĸольĸо у вас источниĸов дохода?",                 new[] { "1", "2-3", "4+" }),
        [QuizStep.SpareMoney]    = ("«Лишние деньги за месяц чаще…",                    new[] { "Инвестирую", "Лежат", "Растворяются" }),
        [QuizStep.ExpenseTracking]=("Учёт расходов ведёте?",                            new[] { "Да, регулярно", "Иногда", "Нет" }),
        [QuizStep.BudgetLeak]    = ("Что сильнее “съедает” бюджет?",                    new[] { "Кредиты", "Спонтанные поĸупĸи", "Бизнес-расходы" }),
        [QuizStep.Reserve]       = ("Резерв поĸрывает…",                                new[] { "< 3 мес", "3–5 мес", "6+ мес" }),
        [QuizStep.Goal]          = ("Главная цель на год?",                             new[] { " Увеличить доход", "Снизить долги", "Наĸопить резерв" }),
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
            await bot.SendMessage(chat, checklist, cancellationToken: ct);

            // ─── отправляем один и тот же PDF ───
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Checklist.pdf");
            await using var fs = File.OpenRead(path);
            
            await bot.SendDocument(
                chat,
                InputFile.FromStream(fs, "Checklist.pdf"),
                checklist,
                cancellationToken: ct);

            states.Reset(chat);
            return;
        }

        // ❹ Шлём следующий вопрос
        var (q, opts) = _map[state.Step];
        await bot.SendMessage(chat, q,
            replyMarkup: BuildReply(opts), cancellationToken: ct);
    }

    private static QuizStep Next(QuizStep step) =>
        step == QuizStep.Goal ? QuizStep.Finished : (QuizStep)((int)step + 1);

    private static Telegram.Bot.Types.ReplyMarkups.ReplyMarkup BuildReply(string[] opts) =>
    opts.Length == 0
        ? new ReplyKeyboardRemove()
        : new ReplyKeyboardMarkup(
              opts.Select(o => new[] { new KeyboardButton(o) }))
        { ResizeKeyboard = true, OneTimeKeyboard = true };


}
