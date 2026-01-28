namespace FaunaFinder.Wildlife.Database.Models;

[Flags]
public enum Behavior
{
    None = 0,
    Feeding = 1,
    Resting = 2,
    Moving = 4,
    Calling = 8
}
