namespace Eggonomics.Models;

public class DebtResult
{
    public string FromDisplayName { get; set; } = string.Empty;
    public string ToDisplayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Label => $"{FromDisplayName} → {ToDisplayName}";
    public string AmountLabel => $"{Amount:F2} CZK";
}
