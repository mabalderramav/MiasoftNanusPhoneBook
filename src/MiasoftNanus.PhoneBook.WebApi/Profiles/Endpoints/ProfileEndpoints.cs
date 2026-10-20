using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Services;
using MiasoftNanus.PhoneBook.WebApi.Profiles.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Profiles.Mappers;

namespace MiasoftNanus.PhoneBook.WebApi.Profiles.Endpoints;

/// <summary>
/// Provides extension methods for mapping profile-related endpoints.
/// </summary>
public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var profileGroup = app.MapGroup("/profiles");
        profileGroup.MapGet("/", GetProfileAll);
        profileGroup.MapGet("/{id:guid}", GetProfileById);
        profileGroup.MapPost("/", CreateProfile);
        profileGroup.MapPut("/{id:guid}", UpdateProfile);
        profileGroup.MapDelete("/{id:guid}", DeleteProfile);
    }

    private static async Task<IResult> GetProfileAll(
        IProfileService profileService,
        CancellationToken cancellationToken)
    {
        var result = await profileService.GetAllAsync(cancellationToken);
        if (result.IsFailure)
            return TypedResults.NotFound(result.Error);

        var profileDtos = result.Value.ToDto();
        return TypedResults.Ok(profileDtos);
    }

    private static async Task<IResult> GetProfileById(
        Guid id,
        IProfileService profileService,
        CancellationToken cancellationToken)
    {
        var result = await profileService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
            return TypedResults.NotFound(result.Error);
        
        var profileDto = result.Value.ToDto();
        return TypedResults.Ok(profileDto);
    }

    private static async Task<IResult> CreateProfile(
        CrearUpdateProfileDto crearUpdateProfileDto,
        IProfileService profileService,
        CancellationToken cancellationToken)
    {
        var profile = crearUpdateProfileDto.ToEntity();
        var result = await profileService.CreateAsync(profile, cancellationToken);
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Created($"/profiles/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateProfile(
        Guid id,
        CrearUpdateProfileDto crearUpdateProfileDto,
        IProfileService profileService,
        CancellationToken cancellationToken)
    {
        var profile = crearUpdateProfileDto.ToEntity();
        var result = await profileService.UpdateAsync(id, profile, cancellationToken);
        return result.IsFailure
            ? TypedResults.NotFound(result.Error)
            : TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> DeleteProfile(
        Guid id,
        IProfileService profileService,
        CancellationToken cancellationToken)
    {
        var result = await profileService.DeleteAsync(id, cancellationToken);
        return result.IsFailure
            ? TypedResults.NotFound(result.Error)
            : TypedResults.NoContent();
    }
}