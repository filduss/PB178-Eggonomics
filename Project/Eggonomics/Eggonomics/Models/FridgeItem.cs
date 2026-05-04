namespace Eggonomics.Models;

public class FridgeItem
{
    public Fridge Fridge { get; init; } = null!;
    public bool IsOwner { get; init; }
}
