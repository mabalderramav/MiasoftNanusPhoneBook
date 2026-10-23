using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Services;
using MiasoftNanus.PhoneBook.WebApi.Agendas.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Agendas.Mappers;

namespace MiasoftNanus.PhoneBook.WebApi.Agendas.Endpoints;

/// <summary>
/// Provides extension methods for mapping agenda-related endpoints.
/// </summary>
public static class AgendaEndpoints
{
    public static void MapAgendaEndpoints(this IEndpointRouteBuilder app)
    {
        var agendaGroup = app.MapGroup("/agendas");
        agendaGroup.MapGet("/", GetAgendaAll);
        agendaGroup.MapGet("/{id:guid}", GetAgendaById);
        agendaGroup.MapPost("/", CreateAgenda);
        agendaGroup.MapPut("/{id:guid}", UpdateAgenda);
        agendaGroup.MapPatch("/{id:guid}", PatchAgenda)
            .Accepts<PatchAgendaDto>("application/merge-patch+json", "application/json");
        agendaGroup.MapDelete("/{id:guid}", DeleteAgenda);
    }

    private static async Task<IResult> GetAgendaAll(
        IAgendaService agendaService,
        CancellationToken cancellationToken)
    {
        var result = await agendaService.GetAllAsync(cancellationToken);
        if (result.IsFailure)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(result.Value.ToDto());
    }

    private static async Task<IResult> GetAgendaById(
        Guid id,
        IAgendaService agendaService,
        CancellationToken cancellationToken)
    {
        var result = await agendaService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(result.Value.ToDto());
    }

    private static async Task<IResult> CreateAgenda(
        CreateUpdateAgendaDto createUpdateAgendaDto,
        IAgendaService agendaService,
        CancellationToken cancellationToken)
    {
        var agendaResult = createUpdateAgendaDto.ToEntity();
        if (agendaResult.IsFailure)
            return TypedResults.BadRequest(agendaResult.Error);

        var result = await agendaService.CreateAsync(agendaResult.Value, cancellationToken);
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Created($"/agendas/{result.Value.Id}", result.Value.ToDto());
    }

    private static async Task<IResult> UpdateAgenda(
        Guid id,
        CreateUpdateAgendaDto createUpdateAgendaDto,
        IAgendaService agendaService,
        CancellationToken cancellationToken)
    {
        var agendaResult = createUpdateAgendaDto.ToEntity();
        if (agendaResult.IsFailure)
            return TypedResults.BadRequest(agendaResult.Error);

        var result = await agendaService.UpdateAsync(id, agendaResult.Value, cancellationToken);
        if (result.IsFailure)
            return ToErrorResult(result.Error);

        return TypedResults.Ok(result.Value.ToDto());
    }

    private static async Task<IResult> PatchAgenda(
        Guid id,
        PatchAgendaDto patchAgendaDto,
        IAgendaService agendaService,
        CancellationToken cancellationToken)
    {
        var result = await agendaService.PatchAsync(id, patchAgendaDto.ToAgendaPatchData(), cancellationToken);
        if (result.IsFailure)
            return ToErrorResult(result.Error);

        return TypedResults.Ok(result.Value.ToDto());
    }

    // A missing agenda is a 404; any other failure (validation, unknown workstation / location, ...) is a 400.
    private static IResult ToErrorResult(Error error)
    {
        return error == AgendaErrors.AgendaNotFound
            ? TypedResults.NotFound(error)
            : TypedResults.BadRequest(error);
    }

    private static async Task<IResult> DeleteAgenda(
        Guid id,
        IAgendaService agendaService,
        CancellationToken cancellationToken)
    {
        var result = await agendaService.DeleteAsync(id, cancellationToken);
        return result.IsFailure
            ? TypedResults.NotFound(result.Error)
            : TypedResults.NoContent();
    }
}
