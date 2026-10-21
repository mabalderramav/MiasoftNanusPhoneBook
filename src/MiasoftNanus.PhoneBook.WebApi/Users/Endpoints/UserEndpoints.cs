using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Domain.Users.Services;
using MiasoftNanus.PhoneBook.WebApi.Users.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Users.Mappers;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Endpoints;

/// <summary>
/// Provides extension methods for mapping user-related endpoints.
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("/users");
        userGroup.MapGet("/", GetUserAll);
        userGroup.MapGet("/{id:guid}", GetUserById);
        userGroup.MapPost("/", CreateUser);
        userGroup.MapPut("/{id:guid}", UpdateUser);
        userGroup.MapDelete("/{id:guid}", DeleteUser);
    }

    private static async Task<IResult> GetUserAll(
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetAllAsync(cancellationToken);
        if (result.IsFailure)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(result.Value.ToDto());
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(result.Value.ToDto());
    }

    private static async Task<IResult> CreateUser(
        CreateUpdateUserDto createUpdateUserDto,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken)
    {
        var userResult = createUpdateUserDto.ToEntity(dateTimeProvider.CurrentTime);
        if (userResult.IsFailure)
            return TypedResults.BadRequest(userResult.Error);

        var result = await userService.CreateAsync(userResult.Value, cancellationToken);
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Created($"/users/{result.Value.Id}", result.Value.ToDto());
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        CreateUpdateUserDto createUpdateUserDto,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken)
    {
        var userResult = createUpdateUserDto.ToEntity(dateTimeProvider.CurrentTime);
        if (userResult.IsFailure)
            return TypedResults.BadRequest(userResult.Error);

        var result = await userService.UpdateAsync(id, userResult.Value, cancellationToken);
        return result.IsFailure
            ? TypedResults.NotFound(result.Error)
            : TypedResults.Ok(result.Value.ToDto());
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.DeleteAsync(id, cancellationToken);
        return result.IsFailure
            ? TypedResults.NotFound(result.Error)
            : TypedResults.NoContent();
    }
}
