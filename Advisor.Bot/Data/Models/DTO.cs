namespace Advisor.Bot.Data.Models;

public class QuizItem
{
    public string Q { get; set; } = default!;
    public string[] Opts { get; set; } = Array.Empty<string>();
}
