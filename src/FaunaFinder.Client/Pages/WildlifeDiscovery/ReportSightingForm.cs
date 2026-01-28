using FluentValidation;

namespace FaunaFinder.Client.Pages.WildlifeDiscovery;

public class ReportSightingForm
{
    public int? SpeciesId { get; set; }
    public string? SpeciesName { get; set; }
    public string Mode { get; set; } = "Casual";
    public string Confidence { get; set; } = "";
    public string Count { get; set; } = "";
    public bool BehaviorFeeding { get; set; }
    public bool BehaviorResting { get; set; }
    public bool BehaviorMoving { get; set; }
    public bool BehaviorCalling { get; set; }
    public bool EvidenceVisual { get; set; }
    public bool EvidenceHeard { get; set; }
    public bool EvidenceTracks { get; set; }
    public bool EvidencePhoto { get; set; }
    public string? Weather { get; set; }
    public string? Notes { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime? ObservedDate { get; set; } = DateTime.Today;
    public TimeSpan? ObservedTime { get; set; } = DateTime.Now.TimeOfDay;

    public int GetBehaviors()
    {
        int behaviors = 0;
        if (BehaviorFeeding) behaviors |= 1;
        if (BehaviorResting) behaviors |= 2;
        if (BehaviorMoving) behaviors |= 4;
        if (BehaviorCalling) behaviors |= 8;
        return behaviors;
    }

    public int GetEvidence()
    {
        int evidence = 0;
        if (EvidenceVisual) evidence |= 1;
        if (EvidenceHeard) evidence |= 2;
        if (EvidenceTracks) evidence |= 4;
        if (EvidencePhoto) evidence |= 8;
        return evidence;
    }

    public DateTime GetObservedAt()
    {
        return ObservedDate!.Value.Date + ObservedTime!.Value;
    }
}

public class ReportSightingFormValidator : AbstractValidator<ReportSightingForm>
{
    public ReportSightingFormValidator()
    {
        RuleFor(x => x.SpeciesId)
            .NotNull().WithMessage("Species is required");

        RuleFor(x => x.Mode)
            .NotEmpty().WithMessage("Mode is required");

        RuleFor(x => x.Confidence)
            .NotEmpty().WithMessage("Confidence level is required");

        RuleFor(x => x.Count)
            .NotEmpty().WithMessage("Count estimate is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.ObservedDate)
            .NotNull().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Observation date cannot be in the future");

        RuleFor(x => x.ObservedTime)
            .NotNull().WithMessage("Time is required");

        // Observation date/time combined cannot be in the future
        RuleFor(x => x)
            .Must(x => !IsObservationInFuture(x))
            .WithMessage("Observation date and time cannot be in the future")
            .When(x => x.ObservedDate.HasValue && x.ObservedTime.HasValue);

        // At least one evidence type is required
        RuleFor(x => x)
            .Must(x => x.GetEvidence() > 0)
            .WithMessage("At least one evidence type is required");
    }

    private static bool IsObservationInFuture(ReportSightingForm form)
    {
        if (!form.ObservedDate.HasValue || !form.ObservedTime.HasValue)
            return false;

        var observedAt = form.ObservedDate.Value.Date + form.ObservedTime.Value;
        return observedAt > DateTime.Now.AddMinutes(5); // 5 minute grace period
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(
            ValidationContext<ReportSightingForm>.CreateWithOptions(
                (ReportSightingForm)model,
                x => x.IncludeProperties(propertyName)));

        return result.IsValid
            ? []
            : result.Errors.Select(e => e.ErrorMessage);
    };
}
