namespace FaunaFinder.Wildlife.Database.Models;

[Flags]
public enum EvidenceType
{
    None = 0,
    Visual = 1,
    Heard = 2,
    Tracks = 4,
    Photo = 8
}
