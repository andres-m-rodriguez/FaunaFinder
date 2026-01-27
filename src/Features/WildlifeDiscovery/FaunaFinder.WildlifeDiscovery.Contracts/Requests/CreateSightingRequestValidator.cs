using FluentValidation;

namespace FaunaFinder.WildlifeDiscovery.Contracts.Requests;

public sealed class CreateSightingRequestValidator : AbstractValidator<CreateSightingRequest>
{
    private static readonly string[] ValidModes = ["Casual", "Survey"];
    private static readonly string[] ValidConfidenceLevels = ["Certain", "FairlySure", "Unsure"];
    private static readonly string[] ValidCountRanges = ["One", "TwoToFive", "SixToTwenty", "TwentyPlus"];
    private static readonly string[] ValidWeatherTypes = ["Clear", "PartlyCloudy", "Cloudy", "Rainy", "Stormy", "Foggy", "Windy"];

    public CreateSightingRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.SpeciesId.HasValue || x.UserSpeciesId.HasValue)
            .WithMessage("Either SpeciesId or UserSpeciesId must be provided");

        RuleFor(x => x.Mode)
            .NotEmpty().WithMessage("Mode is required")
            .Must(m => ValidModes.Contains(m)).WithMessage("Invalid mode");

        RuleFor(x => x.Confidence)
            .NotEmpty().WithMessage("Confidence level is required")
            .Must(c => ValidConfidenceLevels.Contains(c)).WithMessage("Invalid confidence level");

        RuleFor(x => x.Count)
            .NotEmpty().WithMessage("Count range is required")
            .Must(c => ValidCountRanges.Contains(c)).WithMessage("Invalid count range");

        RuleFor(x => x.Behaviors)
            .GreaterThanOrEqualTo(0).WithMessage("Invalid behaviors value");

        RuleFor(x => x.Evidence)
            .GreaterThan(0).WithMessage("At least one evidence type must be selected");

        RuleFor(x => x.Weather)
            .Must(w => w == null || ValidWeatherTypes.Contains(w))
            .WithMessage("Invalid weather type");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.ObservedAt)
            .NotEmpty().WithMessage("Observation date/time is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)).WithMessage("Observation date/time cannot be in the future");
    }
}
