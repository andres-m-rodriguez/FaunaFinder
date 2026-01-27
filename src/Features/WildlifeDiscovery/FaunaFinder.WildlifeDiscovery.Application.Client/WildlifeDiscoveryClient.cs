using System.Net;
using System.Net.Http.Json;
using FaunaFinder.WildlifeDiscovery.Contracts.Errors;
using FaunaFinder.WildlifeDiscovery.Contracts.Requests;
using FaunaFinder.WildlifeDiscovery.Contracts.Responses;
using FaunaFinder.WildlifeDiscovery.Contracts.Results;
using FluentValidation;

namespace FaunaFinder.WildlifeDiscovery.Application.Client;

public sealed class WildlifeDiscoveryClient : IWildlifeDiscoveryClient
{
    private readonly HttpClient _httpClient;
    private readonly IValidator<CreateSightingRequest> _createSightingValidator;
    private readonly IValidator<CreateUserSpeciesRequest> _createUserSpeciesValidator;
    private readonly IValidator<ReviewSightingRequest> _reviewSightingValidator;
    private readonly IValidator<VerifyUserSpeciesRequest> _verifyUserSpeciesValidator;

    public WildlifeDiscoveryClient(
        HttpClient httpClient,
        IValidator<CreateSightingRequest> createSightingValidator,
        IValidator<CreateUserSpeciesRequest> createUserSpeciesValidator,
        IValidator<ReviewSightingRequest> reviewSightingValidator,
        IValidator<VerifyUserSpeciesRequest> verifyUserSpeciesValidator)
    {
        _httpClient = httpClient;
        _createSightingValidator = createSightingValidator;
        _createUserSpeciesValidator = createUserSpeciesValidator;
        _reviewSightingValidator = reviewSightingValidator;
        _verifyUserSpeciesValidator = verifyUserSpeciesValidator;
    }

    public async Task<SearchSpeciesResult> SearchSpeciesAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/wildlife/species/search?query={Uri.EscapeDataString(query)}&limit={limit}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var species = await response.Content.ReadFromJsonAsync<List<SpeciesSearchDto>>(cancellationToken);
                return new SpeciesSearchListResult(species ?? []);
            }

            return new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<CreateSightingResult> CreateSightingAsync(CreateSightingRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createSightingValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/wildlife/sightings", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var sighting = await response.Content.ReadFromJsonAsync<SightingDto>(cancellationToken);
                return sighting!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                HttpStatusCode.NotFound when (await response.Content.ReadAsStringAsync(cancellationToken)).Contains("Species") =>
                    new SpeciesNotFoundError(request.SpeciesId ?? 0),
                HttpStatusCode.NotFound =>
                    new UserSpeciesNotFoundError(request.UserSpeciesId ?? 0),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<GetSightingsResult> GetSightingsAsync(int? page = null, int? pageSize = null, string? status = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/wildlife/sightings";
            var queryParams = new List<string>();
            if (page.HasValue) queryParams.Add($"page={page.Value}");
            if (pageSize.HasValue) queryParams.Add($"pageSize={pageSize.Value}");
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var sightings = await response.Content.ReadFromJsonAsync<List<SightingListDto>>(cancellationToken);
                return new SightingListResult(sightings ?? []);
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<GetSightingResult> GetSightingAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/wildlife/sightings/{id}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var sighting = await response.Content.ReadFromJsonAsync<SightingDto>(cancellationToken);
                return sighting!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                HttpStatusCode.NotFound => new NotFoundError("Sighting", id),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<GetSightingsResult> GetMySightingsAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "api/wildlife/my-sightings";
            var queryParams = new List<string>();
            if (page.HasValue) queryParams.Add($"page={page.Value}");
            if (pageSize.HasValue) queryParams.Add($"pageSize={pageSize.Value}");
            if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var sightings = await response.Content.ReadFromJsonAsync<List<SightingListDto>>(cancellationToken);
                return new SightingListResult(sightings ?? []);
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<ReviewSightingResult> ReviewSightingAsync(int id, ReviewSightingRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _reviewSightingValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/wildlife/sightings/{id}/review", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var sighting = await response.Content.ReadFromJsonAsync<SightingDto>(cancellationToken);
                return sighting!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                HttpStatusCode.Forbidden => new ForbiddenError(),
                HttpStatusCode.NotFound => new NotFoundError("Sighting", id),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<CreateUserSpeciesResult> CreateUserSpeciesAsync(CreateUserSpeciesRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createUserSpeciesValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/wildlife/user-species", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var userSpecies = await response.Content.ReadFromJsonAsync<UserSpeciesDto>(cancellationToken);
                return userSpecies!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<GetUserSpeciesResult> GetUserSpeciesAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/wildlife/user-species/{id}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var userSpecies = await response.Content.ReadFromJsonAsync<UserSpeciesDto>(cancellationToken);
                return userSpecies!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                HttpStatusCode.NotFound => new NotFoundError("UserSpecies", id),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<VerifyUserSpeciesResult> VerifyUserSpeciesAsync(int id, VerifyUserSpeciesRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _verifyUserSpeciesValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(
                "Validation failed",
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/wildlife/user-species/{id}/verify", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var userSpecies = await response.Content.ReadFromJsonAsync<UserSpeciesDto>(cancellationToken);
                return userSpecies!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                HttpStatusCode.Forbidden => new ForbiddenError(),
                HttpStatusCode.NotFound => new NotFoundError("UserSpecies", id),
                HttpStatusCode.BadRequest => new InvalidOperationError(await response.Content.ReadAsStringAsync(cancellationToken)),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public async Task<GetReviewQueueResult> GetReviewQueueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/wildlife/review-queue", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var queue = await response.Content.ReadFromJsonAsync<ReviewQueueDto>(cancellationToken);
                return queue!;
            }

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedError(),
                HttpStatusCode.Forbidden => new ForbiddenError(),
                _ => new UnexpectedError(await response.Content.ReadAsStringAsync(cancellationToken))
            };
        }
        catch (Exception ex)
        {
            return new UnexpectedError(ex.Message);
        }
    }

    public string GetSightingPhotoUrl(int sightingId) => $"api/wildlife/sightings/{sightingId}/photo";

    public string GetSightingAudioUrl(int sightingId) => $"api/wildlife/sightings/{sightingId}/audio";

    public string GetUserSpeciesPhotoUrl(int userSpeciesId) => $"api/wildlife/user-species/{userSpeciesId}/photo";
}
