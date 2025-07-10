using Advisor.Bot.State.Models;

namespace Advisor.Bot.State;

/// <summary>Хранит in-memory состояние пользователей.</summary>
public sealed class StateService
{
    private readonly Dictionary<long, UserState> _users = new();

    public UserState Get(long chatId) =>
        _users.TryGetValue(chatId, out var s) ? s : (_users[chatId] = new());

    public void Reset(long chatId) => _users[chatId] = new();
}
