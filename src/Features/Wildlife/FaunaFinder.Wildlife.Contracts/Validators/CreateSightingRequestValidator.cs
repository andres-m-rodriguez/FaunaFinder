using FluentValidation;

namespace FaunaFinder.Wildlife.Contracts.Validators;

public sealed class CreateSightingRequestValidator : AbstractValidator<CreateSightingRequest>
{
    private static readonly string[] ValidModes = ["Casual", "Survey"];
    private static readonly string[] ValidConfidenceLevels = ["Certain", "FairlySure", "Unsure"];
    private static readonly string[] ValidCountRanges = ["One", "TwoToFive", "SixToTwenty", "TwentyPlus"];
    private static readonly string[] ValidWeatherOptions = ["Clear", "PartlyCloudy", "Cloudy", "Rainy", "Stormy", "Foggy", "Windy"];

    public CreateSightingRequestValidator()
    {
        RuleFor(x => x.SpeciesId)
            .GreaterThan(0)
            .WithMessage("SpeciesId must be greater than 0");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.ObservedAt)
            .NotEmpty()
            .WithMessage("ObservedAt is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("ObservedAt cannot be in the future");

        RuleFor(x => x.Mode)
            .NotEmpty()
            .WithMessage("Mode is required")
            .Must(mode => ValidModes.Contains(mode, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Mode must be one of: Casual, Survey");

        RuleFor(x => x.Confidence)
            .NotEmpty()
            .WithMessage("Confidence is required")
            .Must(conf => ValidConfidenceLevels.Contains(conf, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Confidence must be one of: Certain, FairlySure, Unsure");

        RuleFor(x => x.Count)
            .NotEmpty()
            .WithMessage("Count is required")
            .Must(count => ValidCountRanges.Contains(count, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Count must be one of: One, TwoToFive, SixToTwenty, TwentyPlus");

        RuleFor(x => x.Weather)
            .Must(weather => weather is null || ValidWeatherOptions.Contains(weather, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Weather must be one of: Clear, PartlyCloudy, Cloudy, Rainy, Stormy, Foggy, Windy");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.PhotoContentType)
            .Must(ct => ct is null || ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("PhotoContentType must be an image MIME type");
    }
}
