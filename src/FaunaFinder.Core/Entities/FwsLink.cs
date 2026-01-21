namespace FaunaFinder.Core.Entities;

public class FwsLink
{
    public int Id { get; set; }
    public int NrcsPracticeId { get; set; }
    public int FwsActionId { get; set; }
    public int SpeciesId { get; set; }
    public string? Justification { get; set; }

    public NrcsPractice NrcsPractice { get; set; } = null!;
    public FwsAction FwsAction { get; set; } = null!;
    public Species Species { get; set; } = null!;
}
