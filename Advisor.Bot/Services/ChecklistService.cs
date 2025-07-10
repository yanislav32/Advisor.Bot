using System.Text;
using Advisor.Bot.State.Models;

namespace Advisor.Bot.Services;

public sealed class ChecklistService
{
    public string Build(IReadOnlyDictionary<QuizStep, string> a)
    {
        var sb = new StringBuilder();
        sb.AppendLine("📝 *Ваш чек-лист*").AppendLine();

        if (a.TryGetValue(QuizStep.SpareMoney, out var left) && left == "Нет")
            sb.AppendLine("• Откладывайте не менее 10 % дохода.");

        sb.AppendLine().Append("Успехов! 🚀");
        return sb.ToString();
    }
}
