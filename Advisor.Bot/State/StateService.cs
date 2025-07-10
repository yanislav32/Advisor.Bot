using Advisor.Bot.State.Models;

namespace Advisor.Bot.State;

public sealed class StateService
{
    private readonly Dictionary<long, UserState> _users = [];

    public UserState Get(long chatId) =>
        _users.TryGetValue(chatId, out var s) ? s : (_users[chatId] = new());

    public void Reset(long chatId) => _users[chatId] = new();
}

public sealed class UserState
{
    public QuizStep Step { get; set; } = QuizStep.None;
    public Dictionary<QuizStep, string> Answers { get; } = [];
}
