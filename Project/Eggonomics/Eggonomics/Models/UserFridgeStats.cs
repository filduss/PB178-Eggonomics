namespace Eggonomics.Models;

public class UserFridgeStats
{
    public string DisplayName { get; set; } = string.Empty;
    public int EggsGiven { get; set; }
    public int EggsTaken { get; set; }
    public int EggsExpired { get; set; }
    public decimal ValueGiven { get; set; }
    public decimal ValueTaken { get; set; }
    public decimal ValueExpired { get; set; }
    public decimal NetValue => ValueGiven - ValueTaken - ValueExpired;
    public Color NetValueColor => NetValue >= 0 ? Colors.Green : Colors.Red;
}
