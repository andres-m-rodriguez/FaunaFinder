namespace FaunaFinder.Core.Entities;

public class FwsAction
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<FwsLink> FwsLinks { get; set; } = new List<FwsLink>();
}
