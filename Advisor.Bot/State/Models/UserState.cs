using System.Collections.Generic;

namespace Advisor.Bot.State.Models;

public sealed class UserState
{
    public QuizStep Step { get; set; } = QuizStep.None;
    public Dictionary<QuizStep, string> Answers { get; } = new();
}
