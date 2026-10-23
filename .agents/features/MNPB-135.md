# MNPB-135 WebApi - Refactor: Agenda partial update (PATCH) and embedded references

## Summary

Pick up two open items from MNPB-126:

> **MNPB-126 — Out of scope:**
> - PATCH / enable-disable endpoint.
> - Embedding `Workstation` / `Location` / `UserCreated` in the payload (only ids).

After this ticket:

| Verb   | Route                | Body                       | Success              | Failure                                  |
|--------|----------------------|----------------------------|----------------------|------------------------------------------|
| GET    | `/agendas`           | –                          | 200 `AgendaDto[]` ¹  | 404 `Error`                              |
| GET    | `/agendas/{id:guid}` | –                          | 200 `AgendaDto` ¹    | 404 `Error`                              |
| POST   | `/agendas`           | `CreateUpdateAgendaDto`    | 201 `AgendaDto` ¹    | 400 `Error`                              |
| PUT    | `/agendas/{id:guid}` | `CreateUpdateAgendaDto`    | 200 `AgendaDto` ¹    | 404 `AgendaNotFound` / 400 other ²       |
| PATCH  | `/agendas/{id:guid}` | `PatchAgendaDto` (**new**) | 200 `AgendaDto` ¹    | 404 `AgendaNotFound` / 400 other `Error` |
| DELETE | `/agendas/{id:guid}` | –                          | 204                  | 404 `Error`                              |

¹ `AgendaDto` keeps `workstationId` / `locationId` / `userIdCreated` and **adds** the embedded `workstation`,
`location` and `userCreated` objects (additive change, existing clients keep working).
² Behaviour change: today every PUT failure is a 404 (MNPB-126 Notes, "Error mapping").

- `PATCH /agendas/{id}` follows **JSON Merge Patch** (RFC 7396) like `PATCH /users/{id}` (MNPB-133): every field is
  optional, an absent (or `null`) field keeps its current value. The merged result goes through the same
  `Agenda.Validate` as POST / PUT.
- **Enable / disable** is a PATCH: `{ "enabled": false }`. No dedicated route (see Motivation).
- Optional phone / image fields are **cleared with `""`** (see Motivation, "Clearing optional fields").
- `userIdCreated` stays immutable and is not part of the PATCH body.
- No DB schema change. No new packages.

Example:

```http
PATCH /agendas/0192f3a4-...
Content-Type: application/merge-patch+json

{ "enabled": false, "internal": "2045", "corporate": "" }
```

→ disables the contact, changes the internal extension, clears the corporate line; everything else is kept.

```json
{
  "id": "0192f3a4-...",
  "code": "A-001",
  "fullName": "Juan Pérez",
  "email": "juan.perez@miasoft.com",
  "enabled": false,
  "imagePath": null,
  "directLine": "3-3456789",
  "internal": "2045",
  "corporate": null,
  "directAccess": null,
  "workstationId": "0192...",
  "locationId": "0192...",
  "userIdCreated": "0192...",
  "workstation": { "id": "0192...", "code": "WS-01", "name": "Sistemas", "description": null, "workAreaId": "0192..." },
  "location": { "id": "0192...", "code": "SCZ", "description": "Santa Cruz" },
  "userCreated": { "id": "0192...", "userName": "mbalderrama", "firstName": "Miguel", "lastName": "Balderrama" }
}
```

## Motivation

### PATCH

`PUT /agendas/{id}` is a full replace of 12 fields. The most frequent edits on a phone book entry are small:
enable / disable a contact, change one extension, move it to another workstation or location. Today the client must
`GET` the agenda and send everything back, which is chatty and races (two operators editing different phones
overwrite each other with stale values). MNPB-133 already established the pattern for `User`; this ticket applies
it to `Agenda`, which is the resource where partial edits are most common.

### Enable / disable: PATCH, not a dedicated route

| Option                                              | Verdict |
|-----------------------------------------------------|---------|
| **`PATCH /agendas/{id}` with `{ "enabled": ... }`** | ✅ chosen |
| `PUT /agendas/{id}/enabled` (`{ "enabled": bool }`) | ❌ |
| `POST /agendas/{id}/enable` + `/disable`            | ❌ |

- `Enabled` has no rules of its own (unlike the user password in MNPB-130, which needed hashing and its own
  validation), so a dedicated route adds a command, handler and endpoint for a one-field PATCH.
- Same reasoning as MNPB-133 ("dedicated routes do not scale").
- If enabling / disabling later needs its own behaviour (domain event, audit, permission), add
  `Agenda.Enable()` / `Disable()` and a route then; the PATCH keeps working.

### Clearing optional fields: `""` clears, `null` / absent keeps

`Agenda` — unlike `User` — has optional fields (`ImagePath`, `DirectLine`, `Internal`, `Corporate`,
`DirectAccess`), so the open question in MNPB-133 Notes ("`null` semantics") must be answered here: how does a
client *remove* a phone line?

| Option                                                                 | Verdict |
|------------------------------------------------------------------------|---------|
| **Nullable DTO; `null` / absent = keep, `""` = clear**                  | ✅ chosen |
| `Optional<T>` wrapper + `JsonConverter` (strict RFC 7396: `null` = remove) | ❌ |
| Bind body as `JsonObject` and probe with `TryGetPropertyValue`         | ❌ |

1. **Already the domain rule.** `Agenda.Apply` stores optional strings through `NullIfBlank`, so `""` and `"  "`
   already mean "no value" on POST / PUT (MNPB-126). PATCH just passes `""` through: `"" ?? current` is `""`, and
   `Apply` turns it into `null`.
2. **Same DTO style as `PatchUserDto`.** Plain nullable positional record, normal OpenAPI schema, no custom
   converter, no schema transformer for `Optional<T>`.
3. **Consistent `null` meaning across the API.** In `PATCH /users`, `null` = keep. Making `null` = remove only on
   agendas would give the same JSON two meanings depending on the resource.
4. **Required fields stay safe.** `"code": ""`, `"fullName": ""`, `"email": ""` reach `Validate` and are rejected
   (400), exactly as on PUT.

This is a documented deviation from RFC 7396 (same as MNPB-133); see Notes.

### Embedding `Workstation` / `Location` / `UserCreated`

A phone book listing shows the workstation name and the location, not their GUIDs. Today a client listing 200
agendas must call `GET /workstations` and `GET /locations` and join client-side, and cannot show who created an
entry at all without `GET /users/{id}` per row.

| Option                                                                | Verdict |
|-----------------------------------------------------------------------|---------|
| **Always embed small objects, keep the flat ids**                     | ✅ chosen |
| Replace the ids with the objects                                      | ❌ breaking |
| `?expand=workstation,location,userCreated` query parameter            | ❌ for now |

- **Additive:** the three ids stay, so PUT / POST bodies and existing readers are unchanged; the objects are new
  properties.
- **Small and bounded:** one level only (no `workArea` inside `workstation`, no `profile` inside `userCreated`).
  Three `LEFT JOIN`s on a bounded table are cheaper than three extra round trips.
- `?expand=` is the flexible answer, but it needs parsing, per-request `Include` composition and conditional DTO
  members. Not worth it while every client wants the same three references.
- **What is embedded:**
  - `workstation` → existing `WorkstationDto` (`id`, `code`, `name`, `description`, `workAreaId`).
  - `location` → existing `LocationDto` (`id`, `code`, `description`).
  - `userCreated` → **new** `UserSummaryDto` (`id`, `userName`, `firstName`, `lastName`). The full `UserDto`
    exposes birthdate, address and email; a phone book entry should not leak them.

### Where the loading happens

`CrudRepository.GetByIdAsync` / `GetAllAsync` do not load navigations, and `GetByIdAsync` is **tracked** (comment in
`CrudRepository`: needed for the `xmin` token). Adding `Include` to the tracked read used by the command handlers is
risky: `DbSet.Update(agenda)` walks the graph and the parents could be attached as `Modified` (extra `UPDATE`s on
`phonebook_workstations` / `location` / `phonebook_users` with their own `xmin` checks). So:

```
Queries (GET)
  → agendaRepository.GetByIdWithDetailsAsync / GetAllWithDetailsAsync   // AsNoTracking + Include × 3
Commands (POST / PUT / PATCH)
  → unchanged tracked GetByIdAsync → Create / Update / Patch → SaveChanges
  → agendaRepository.LoadDetailsAsync(agenda)                           // after save, only refs not yet loaded
```

`LoadDetailsAsync` uses `Entry(agenda).Reference(...)` and skips references that are already loaded. Parents
fetched by the handler for the existence checks (MNPB-126) are already tracked in the same context, so EF fix-up
usually fills them for free and only the missing ones hit the DB.

## Implementation

Work bottom-up. The build should be green after each step (Steps 1 and 2 together: the new
`IAgendaRepository` members need their implementation).

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Agendas`)

1. **`Entities/AgendaPatchData.cs`** (new):

```csharp
namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents a partial update of an agenda. A <c>null</c> member keeps the current value. For the optional
/// phone / image members, an empty or blank string clears the value. The creator user is not patchable.
/// </summary>
public sealed class AgendaPatchData
{
    public string? Code { get; init; }
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public bool? Enabled { get; init; }
    public string? ImagePath { get; init; }
    public string? DirectLine { get; init; }
    public string? Internal { get; init; }
    public string? Corporate { get; init; }
    public string? DirectAccess { get; init; }
    public Guid? WorkstationId { get; init; }
    public Guid? LocationId { get; init; }

    /// <summary>
    /// Gets whether at least one member is set.
    /// </summary>
    public bool HasChanges =>
        Code is not null || FullName is not null || Email is not null || Enabled is not null
        || ImagePath is not null || DirectLine is not null || Internal is not null || Corporate is not null
        || DirectAccess is not null || WorkstationId is not null || LocationId is not null;
}
```

2. **`Entities/Agenda.cs`** — add `Patch` next to `Update`:

```csharp
/// <summary>
/// Applies a partial update: every member of <paramref name="data"/> that is set replaces the current value, the
/// others are kept. The merged data is validated like <see cref="Update"/>. The creator user is not changed.
/// </summary>
/// <param name="data">The values to change.</param>
public Result Patch(AgendaPatchData data)
{
    return Update(new AgendaCreationData
    {
        Code = data.Code ?? Code,
        FullName = data.FullName ?? FullName,
        Email = data.Email ?? Email!.Value,
        Enabled = data.Enabled ?? Enabled,
        ImagePath = data.ImagePath ?? ImagePath,
        DirectLine = data.DirectLine ?? DirectLine,
        Internal = data.Internal ?? Internal,
        Corporate = data.Corporate ?? Corporate,
        DirectAccess = data.DirectAccess ?? DirectAccess,
        WorkstationId = data.WorkstationId ?? WorkstationId,
        LocationId = data.LocationId ?? LocationId,
        UserIdCreated = UserIdCreated
    });
}
```

   - `Update` validates before applying → a failed patch leaves the entity untouched.
   - `"corporate": ""` → `"" ?? Corporate` = `""` → `Apply` → `NullIfBlank` → `null`. No special case needed.
   - `UserIdCreated = UserIdCreated` only satisfies `required` / `Validate`; `Apply` never writes it.

3. **`Services/IAgendaService.cs`** — from a one-line interface to:

```csharp
public interface IAgendaService : ICrudService<Agenda, Guid>
{
    /// <summary>
    /// Asynchronously applies a partial update to an existing agenda.
    /// </summary>
    /// <param name="id">The unique identifier of the agenda to be updated.</param>
    /// <param name="data">The values to change; unset members keep their current value.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the updated agenda, or an error if the operation failed.</returns>
    Task<Result<Agenda>> PatchAsync(Guid id, AgendaPatchData data, CancellationToken cancellationToken = default);
}
```

4. **`Repositories/IAgendaRepository.cs`** — add the detail reads:

```csharp
public interface IAgendaRepository : IGenericRepository<Agenda, Guid>
{
    /// <summary>
    /// Retrieves an agenda by its identifier with its workstation, location and creator user loaded.
    /// The result is not tracked.
    /// </summary>
    Task<Agenda?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all agendas with their workstation, location and creator user loaded. The result is not tracked.
    /// </summary>
    Task<ICollection<Agenda>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the workstation, location and creator user of a tracked agenda, skipping the ones already loaded.
    /// </summary>
    Task LoadDetailsAsync(Agenda agenda, CancellationToken cancellationToken = default);
}
```

### Step 2 — Infrastructure (`MiasoftNanus.PhoneBook.Infrastructure/Agendas`)

**`AgendaRepository.cs`** — from a one-line class to:

```csharp
public class AgendaRepository(AppDbContext appDbContext) : CrudRepository<Agenda>(appDbContext), IAgendaRepository
{
    public async Task<Agenda?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<ICollection<Agenda>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await WithDetails().ToListAsync(cancellationToken);
    }

    public async Task LoadDetailsAsync(Agenda agenda, CancellationToken cancellationToken = default)
    {
        var entry = AppDbContext.Entry(agenda);
        // No-op for references already loaded or fixed up from parents tracked by the handler.
        await entry.Reference(a => a.Workstation).LoadAsync(cancellationToken);
        await entry.Reference(a => a.Location).LoadAsync(cancellationToken);
        await entry.Reference(a => a.UserCreated).LoadAsync(cancellationToken);
    }

    private IQueryable<Agenda> WithDetails()
    {
        return AppDbContext.Set<Agenda>()
            .AsNoTracking()
            .Include(a => a.Workstation)
            .Include(a => a.Location)
            .Include(a => a.UserCreated);
    }
}
```

- `ReferenceEntry.LoadAsync` checks `IsLoaded` itself; if a reference was fixed up (not "loaded" from EF's point of
  view) it may re-query once. Acceptable; verify in the EF log and add `if (!entry.Reference(...).IsLoaded)` plus
  a null-navigation check if it issues redundant queries.
- Navigations are `{ get; init; }`; EF sets them through the backing field (already the case for fix-up today).
- Loading `UserCreated` materialises the whole `User` (incl. password hash) in memory. It never leaves the server
  (`UserSummaryDto`); a projection is a possible later optimisation (Notes).

### Step 3 — Application (`MiasoftNanus.PhoneBook.Application/Agendas`)

1. **`UseCases/Commands/PatchAgenda/PatchAgendaCommand.cs`** (new):

```csharp
/// <summary>
/// Represents a command to partially update an agenda.
/// </summary>
public sealed record PatchAgendaCommand(Guid Id, AgendaPatchData Data) : ICommand<Result<Agenda>>;
```

2. **`UseCases/Commands/PatchAgenda/PatchAgendaCommandHandler.cs`** (new) — same shape as
   `PatchUserCommandHandler`:

```csharp
/// <summary>
/// Applies and persists a partial update of an existing agenda.
/// </summary>
public sealed class PatchAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IWorkstationRepository workstationRepository,
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<PatchAgendaCommand, Result<Agenda>>
{
    public async Task<Result<Agenda>> HandleAsync(
        PatchAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<Agenda>(AgendaErrors.AgendaNotFound);

        // Nothing to change: no save (it would affect 0 rows).
        if (!command.Data.HasChanges)
        {
            await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);
            return Result.Success(agenda);
        }

        var patchResult = agenda.Patch(command.Data);
        if (patchResult.IsFailure)
            return Result.Failure<Agenda>(patchResult.Error);

        if (command.Data.WorkstationId is not null)
        {
            var workstation =
                await workstationRepository.GetByIdAsync(command.Data.WorkstationId.Value, cancellationToken);
            if (workstation is null)
                return Result.Failure<Agenda>(WorkstationErrors.WorkstationNotFound);
        }

        if (command.Data.LocationId is not null)
        {
            var location = await locationRepository.GetByIdAsync(command.Data.LocationId.Value, cancellationToken);
            if (location is null)
                return Result.Failure<Agenda>(LocationErrors.LocationNotFound);
        }

        agendaRepository.Update(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure<Agenda>(Error.CannotSaveChanges);

        await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);
        return Result.Success(agenda);
    }
}
```

   - Parents are only checked when the patch sends their id; unchanged ones were valid when set.
   - Picked up by the assembly scan in `Application/DependencyInjection.AddApplication`.

3. **`Services/AgendaService.cs`** — add:

```csharp
public Task<Result<Agenda>> PatchAsync(Guid id, AgendaPatchData data, CancellationToken cancellationToken = default)
{
    return Dispatcher.SendCommandAsync(new PatchAgendaCommand(id, data), cancellationToken);
}
```

4. **Queries** — switch to the detail reads:
   - `GetAgendaByIdQueryHandler` → `agendaRepository.GetByIdWithDetailsAsync(query.Id, ...)`.
   - `GetAgendaAllQueryHandler` → `agendaRepository.GetAllWithDetailsAsync(...)`.

5. **`CreateAgendaCommandHandler` / `UpdateAgendaCommandHandler`** — after a successful save, call
   `await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);` before returning, so POST / PUT responses
   also carry the embedded objects. Restructure the final ternary into `if (result == 0) return ...;` as in the
   PATCH handler above.

6. **No change** to `DeleteAgenda*`, `AgendaCommandMapper`, `CrudCqrsService`.

### Step 4 — WebApi

1. **`Users/Dtos/UserSummaryDto.cs`** (new):

```csharp
/// <summary>
/// Minimal, non-sensitive view of a user, used when a user is embedded in another resource.
/// </summary>
public record UserSummaryDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName
);
```

   **`Users/Mappers/UserMap.cs`** — add:

```csharp
public static UserSummaryDto ToSummaryDto(this User user)
{
    return new UserSummaryDto(
        user.Id,
        user.UserName!.Value,
        user.FirstName!,
        user.LastName!
    );
}
```

2. **`Agendas/Dtos/AgendaDto.cs`** — append three nullable members (nullable because a navigation may not be
   loaded; with the loading above they always are):

```csharp
public record AgendaDto(
    Guid Id,
    // ... existing members unchanged ...
    Guid WorkstationId,
    Guid LocationId,
    Guid UserIdCreated,
    WorkstationDto? Workstation,
    LocationDto? Location,
    UserSummaryDto? UserCreated
);
```

3. **`Agendas/Dtos/PatchAgendaDto.cs`** (new):

```csharp
/// <summary>
/// Data transfer object used for partially updating an agenda (JSON Merge Patch). A missing or <c>null</c> member
/// keeps the current value; an empty string clears an optional phone / image member.
/// </summary>
public record PatchAgendaDto(
    string? Code,
    string? FullName,
    string? Email,
    bool? Enabled,
    string? ImagePath,
    string? DirectLine,
    string? Internal,
    string? Corporate,
    string? DirectAccess,
    Guid? WorkstationId,
    Guid? LocationId
);
```

   No `UserIdCreated`: a `userIdCreated` in the body is ignored (unknown member), like `password` in
   `PatchUserDto`.

4. **`Agendas/Mappers/AgendaMap.cs`**
   - `ToDto(Agenda)` — add `agenda.Workstation?.ToDto()`, `agenda.Location?.ToDto()`,
     `agenda.UserCreated?.ToSummaryDto()` (reuses `WorkstationMap` / `LocationMap` / `UserMap`).
   - New `ToAgendaPatchData(this PatchAgendaDto)` — field-by-field copy, no `Result`, no validation (same as
     `UserMap.ToUserPatchData`).

5. **`Agendas/Endpoints/AgendaEndpoints.cs`**

```csharp
agendaGroup.MapPut("/{id:guid}", UpdateAgenda);
agendaGroup.MapPatch("/{id:guid}", PatchAgenda)
    .Accepts<PatchAgendaDto>("application/merge-patch+json", "application/json");
agendaGroup.MapDelete("/{id:guid}", DeleteAgenda);
```

```csharp
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
```

   - `UpdateAgenda` also returns `ToErrorResult(result.Error)` instead of `TypedResults.NotFound(...)`.

### Step 5 — Verify

- `dotnet build` the solution.
- `dotnet ef migrations add Check_MNPB135` must produce **no model changes**. Discard it afterwards.
- Run the WebApi (Development) with EF command logging on, and use OpenAPI / a `.http` file. Seed: work area →
  2 workstations → 2 locations → profile → user → `POST /agendas` with all phones set.
- **Embedding**
  - `POST` → 201; body has `workstation`, `location`, `userCreated` filled and matching the ids.
  - `GET /agendas/{id}` and `GET /agendas` → same objects; one SQL query with three `LEFT JOIN`s, no N+1.
  - `userCreated` has only `id`, `userName`, `firstName`, `lastName` (no email / address / password).
  - `PUT` changing `workstationId` → response `workstation` is the **new** one.
  - EF log on PUT / PATCH shows `UPDATE agendas` only — **no** `UPDATE` on `phonebook_workstations`, `location`,
    `phonebook_users`.
- **PATCH**
  - `{ "enabled": false }` → 200; only `enabled` changed.
  - `{ "internal": "2045" }` → only `internal` changed.
  - `{ "corporate": "" }` and `{ "corporate": "   " }` → `corporate` is `null`.
  - `{ "corporate": null }` → treated like `{}`; nothing changed.
  - `{ "locationId": "<other location>" }` → 200; `locationId` and embedded `location` updated.
  - `{ "userIdCreated": "<other user>" }` → 200 no-op; creator unchanged.
  - `{}` → 200 with the current agenda (embedded objects filled); no `UPDATE` in the EF log.
  - `Content-Type: application/merge-patch+json` behaves like `application/json`.
  - Negative cases:
    - `{ "code": "" }` → 400 `AgendaErrors.CodeCannotBeNullOrWhitespace`; DB unchanged.
    - `{ "fullName": " " }` → 400 `AgendaErrors.FullNameCannotBeNullOrWhitespace`.
    - `{ "email": "nope" }` → 400 `UserErrors.InvalidEmail` (shared `Email` VO, see MNPB-126 Notes).
    - `{ "workstationId": "00000000-0000-0000-0000-000000000000" }` → 400 `AgendaErrors.WorkstationIdCannotBeEmpty`.
    - `{ "workstationId": "<unknown guid>" }` → 400 `WorkstationErrors.WorkstationNotFound`.
    - `{ "locationId": "<unknown guid>" }` → 400 `LocationErrors.LocationNotFound`.
    - `PATCH /agendas/{unknown-id}` → 404 `AgendaErrors.AgendaNotFound`.
    - `Content-Type: text/plain` → 415.
- **PUT error mapping**: `PUT` with unknown `workstationId` → **400** (was 404); unknown agenda id → 404.
- Regression: `POST`, `GET`, `DELETE` agendas behave as in MNPB-126; `/users` PATCH untouched.

## Scope

**In scope**
- Domain: `AgendaPatchData`, `Agenda.Patch`, `IAgendaService.PatchAsync`, `IAgendaRepository` detail reads +
  `LoadDetailsAsync`.
- Infrastructure: `AgendaRepository` implementations (`AsNoTracking` + `Include`, reference loading).
- Application: `PatchAgendaCommand` + handler, `AgendaService.PatchAsync`, queries on detail reads, Create /
  Update handlers load details after save.
- WebApi: `PatchAgendaDto`, `UserSummaryDto` + `UserMap.ToSummaryDto`, `AgendaDto` embedded members,
  `AgendaMap` updates, `PATCH /agendas/{id:guid}`, `AgendaEndpoints.ToErrorResult` used by PATCH and PUT.

**Out of scope**
- Dedicated enable / disable routes and domain events for them.
- Strict RFC 7396 `null` = remove (`Optional<T>` / `JsonObject` binding).
- `?expand=` / selectable embedding; nested `workArea` / `profile`.
- Removing the flat `workstationId` / `locationId` / `userIdCreated` from `AgendaDto`.
- Search / filtering / pagination on `GET /agendas` (still the main follow-up from MNPB-126).
- Optimistic concurrency exposure (`xmin` / `ETag` / `If-Match`).
- Rejecting unknown properties (e.g. `userIdCreated` in PATCH) with 400.
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **`null` semantics**: deviation from RFC 7396, consistent with `PATCH /users` (MNPB-133). If a client ever needs
  to distinguish "set to null" from "absent" for a non-string field (e.g. a nullable FK), revisit with an
  `Optional<T>` + `JsonConverterFactory` (value-type wrapper so `HandleNull` is on) and an OpenAPI schema
  transformer — and change `PATCH /users` at the same time.
- **Shared PATCH plumbing**: Users and Agendas now share the same shape (`<Entity>PatchData` + `HasChanges`,
  `<Entity>.Patch` → `Update`, `Patch<Entity>Command(Id, Data)`, `ToErrorResult`). A generic `ToErrorResult` /
  `Error` → `IResult` helper (open since MNPB-120) is now worth extracting; a generic `ICrudService.PatchAsync` still
  is not.
- **Embedding cost**: the `User` row is fully materialised to build `UserSummaryDto`. If `GET /agendas` gets slow
  (or once pagination lands), project straight to a read model in the query handler instead of loading entities.
- **Delete of parents**: with `workstation` / `location` now visible on every agenda, the `...InUse` → 409 check
  before deleting a referenced workstation / location / user (MNPB-126 Notes) becomes more visible to users.
- **Lost updates**: PATCH narrows but does not remove the race; `xmin` → `ETag` + `If-Match` → 412 remains the
  follow-up for PUT and PATCH on both resources.
- Suggested commit split: (1) Domain + Infrastructure + Application PATCH, (2) WebApi PATCH + error mapping,
  (3) embedded references (repository detail reads, handlers, DTOs).
