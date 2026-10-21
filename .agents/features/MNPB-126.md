# MNPB-126 WebApi - Implements Crud Agenda enpoints

## Summary

Expose CRUD HTTP endpoints for `Agenda` under `/agendas` in `MiasoftNanus.PhoneBook.WebApi.Agendas`,
mirroring the User slice delivered in MNPB-124
(`WebApi/Agendas/{Dtos,Endpoints,Mappers}` → `IAgendaService` → CQRS use cases → `IAgendaRepository`).

| Verb   | Route                | Service call               | Success                              | Failure           |
|--------|----------------------|----------------------------|--------------------------------------|-------------------|
| GET    | `/agendas`           | `GetAllAsync`              | 200 `AgendaDto[]`                    | 404 `Error`       |
| GET    | `/agendas/{id:guid}` | `GetByIdAsync(id)`         | 200 `AgendaDto`                      | 404 `Error`       |
| POST   | `/agendas`           | `CreateAsync(agenda)`      | 201 `AgendaDto` + `Location` header  | 400 `Error`       |
| PUT    | `/agendas/{id:guid}` | `UpdateAsync(id, agenda)`  | 200 `AgendaDto`                      | 400 / 404 `Error` |
| DELETE | `/agendas/{id:guid}` | `DeleteAsync(id)`          | 204                                  | 404 `Error`       |

## Motivation

`Agenda` is the core resource of the phone book (a contact with its phone lines, workstation and location) and the
last entity without endpoints. Its parents are now all manageable through the API:
`Workstation` (MNPB-122), `Location` (MNPB-116) and `User` (MNPB-124, referenced by `UserIdCreated`).
The repository (`AgendaRepository`) and EF configuration (`AgendaConfiguration`, table `agendas`) already exist and
`IAgendaRepository` is registered in `Infrastructure/DependencyInjection.cs`.

The current domain entity, however, predates the `Create` / `Update` / `Validate` pattern and has real bugs that
would surface as soon as it is reachable from HTTP:

- `Agenda.Create` **never assigns `Id`** → every agenda is created with `Guid.Empty` (second insert → PK violation).
- `Agenda.Create` **never assigns `WorkstationId` / `LocationId` / `UserIdCreated`** (not in `AgendaCreationData`)
  → FK violations on insert.
- `Email` is nullable in the entity / creation data, but the column is `email text NOT NULL`
  (`AgendaConfiguration` → `.IsRequired()`, and the converter does `email!.Value`) → null email = 500.
- FK properties are `Guid?` while the columns are `uuid NOT NULL` (`IsRequired()` in config / snapshot).
- `init` / `required` properties → no `Update` possible.
- `AgendaErrors` has no `AgendaNotFound` / `AgendasNotFound`.

**Gap analysis vs. Users**

| Layer          | Users (exists)                                                                     | Agendas (status)                                                    |
|----------------|------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| Domain entity  | private ctor, `private set`, `Create` + `Update` + shared `Validate`/`Apply`, `CreateVersion7` | `required init` props, `Create` only, **no `Id`**, **no FKs set**, minimal validation |
| Domain data    | `UserCreationData` with primitives (`required init`)                               | `AgendaCreationData` positional record with `Email?` VO, **no FK ids** |
| Domain errors  | `UserNotFound`, `UsersNotFound` + validation errors                                | only `CodeCannotBeNullOrWhitespace`, `FullNameCannotBeNullOrWhitespace` |
| Domain service | `IUserService : ICrudService<User, Guid>`                                          | **missing** `IAgendaService`                                        |
| Repository     | ✅ `IUserRepository` / `UserRepository` (registered)                               | ✅ `IAgendaRepository` / `AgendaRepository` (registered)            |
| Application    | 5 use cases + `UserCommandMapper` + `UserService` + DI registration                | **missing** all                                                     |
| WebApi         | Dtos, Mapper, Endpoints, `MapUserEndpoints()`                                      | **missing** all                                                     |

Shape of `Agenda` (from `AgendaConfiguration` / snapshot — no schema change planned):

| Property        | Type          | Column                                    | Notes                                         |
|-----------------|---------------|-------------------------------------------|-----------------------------------------------|
| `Code`          | `string`      | `code` varchar(50), req                   |                                               |
| `FullName`      | `string`      | `full_name` varchar(200), req             |                                               |
| `Email`         | `Email` VO    | `email` text, req                         | regex validated → **make required**           |
| `Enabled`       | `bool`        | `enabled` boolean, req                    |                                               |
| `ImagePath`     | `string?`     | `image_path` varchar(300)                 | path / URL only, no upload in this ticket     |
| `DirectLine`    | `string?`     | `direct_line` varchar(50)                 |                                               |
| `Internal`      | `string?`     | `internal` varchar(50)                    |                                               |
| `Corporate`     | `string?`     | `corporate` varchar(50)                   |                                               |
| `DirectAccess`  | `string?`     | `direct_access` varchar(50)               |                                               |
| `WorkstationId` | `Guid`        | `workstation_id` FK → `phonebook_workstations` (NoAction) |                               |
| `LocationId`    | `Guid`        | `location_id` FK → `location` (NoAction)  |                                               |
| `UserIdCreated` | `Guid`        | `user_id_created` FK → `phonebook_users` (NoAction) | no auth yet → comes from the body   |
| `version`       | `uint` shadow | `xmin` (row version / concurrency token)  | not exposed                                   |

## Implementation

Work bottom-up, one commit per layer (builds green after each step).

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Agendas`)

1. **`Entities/AgendaCreationData.cs`** — switch to the `UserCreationData` shape (primitives, `required init`),
   reused by `Create` and `Update`:

```csharp
public sealed class AgendaCreationData
{
    public required string Code { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required bool Enabled { get; init; }
    public string? ImagePath { get; init; }
    public string? DirectLine { get; init; }
    public string? Internal { get; init; }
    public string? Corporate { get; init; }
    public string? DirectAccess { get; init; }
    public required Guid WorkstationId { get; init; }
    public required Guid LocationId { get; init; }
    public required Guid UserIdCreated { get; init; }
}
```

2. **`Entities/Agenda.cs`** — refactor to the User shape:
   - `required init` → `private set` on every mapped property; drop the field-by-field initialization in the
     private ctor (keep an empty **private** parameterless ctor for EF, same as `User`).
   - FK properties `Guid?` → `Guid` (columns are already `NOT NULL`; no migration expected — verify in Step 4).
   - `Create(AgendaCreationData data)`: `Validate` → `new Agenda { Id = Guid.CreateVersion7() }` → `Apply` →
     raise `AgendaCreatedDomainEvent(agenda.Id)` (now with the real id, previously `Guid.Empty`).
   - New `Result Update(AgendaCreationData data)`: same validation; updates every field **except `UserIdCreated`**
     (creator is set once at creation — see Notes).
   - Trim optional phone strings and store `null` for blank values, so `""` and `null` mean the same thing.

```csharp
public class Agenda : Entity
{
    private Agenda()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public bool Enabled { get; private set; }
    public string? ImagePath { get; private set; }
    public string? DirectLine { get; private set; }
    public string? Internal { get; private set; }
    public string? Corporate { get; private set; }
    public string? DirectAccess { get; private set; }
    public Guid WorkstationId { get; private set; }
    public Workstation? Workstation { get; init; }
    public Guid LocationId { get; private set; }
    public Location? Location { get; init; }
    public Guid UserIdCreated { get; private set; }
    public User? UserCreated { get; init; }

    public static Result<Agenda> Create(AgendaCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return Result.Failure<Agenda>(validationResult.Error);

        var agenda = new Agenda { Id = Guid.CreateVersion7(), UserIdCreated = data.UserIdCreated };
        agenda.Apply(data);

        agenda.AddDomainEvent(new AgendaCreatedDomainEvent(agenda.Id));
        return Result.Success(agenda);
    }

    public Result Update(AgendaCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return validationResult;

        Apply(data);
        return Result.Success();
    }

    private void Apply(AgendaCreationData data)
    {
        Code = data.Code;
        FullName = data.FullName;
        Email = Email.Create(data.Email).Value;
        Enabled = data.Enabled;
        ImagePath = NullIfBlank(data.ImagePath);
        DirectLine = NullIfBlank(data.DirectLine);
        Internal = NullIfBlank(data.Internal);
        Corporate = NullIfBlank(data.Corporate);
        DirectAccess = NullIfBlank(data.DirectAccess);
        WorkstationId = data.WorkstationId;
        LocationId = data.LocationId;
    }

    private static Result Validate(AgendaCreationData data)
    {
        if (string.IsNullOrWhiteSpace(data.Code))
            return Result.Failure(AgendaErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(data.FullName))
            return Result.Failure(AgendaErrors.FullNameCannotBeNullOrWhitespace);

        var emailResult = Email.Create(data.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        if (data.WorkstationId == Guid.Empty)
            return Result.Failure(AgendaErrors.WorkstationIdCannotBeEmpty);

        if (data.LocationId == Guid.Empty)
            return Result.Failure(AgendaErrors.LocationIdCannotBeEmpty);

        if (data.UserIdCreated == Guid.Empty)
            return Result.Failure(AgendaErrors.UserIdCreatedCannotBeEmpty);

        return Result.Success();
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
```

   > `Entity.Id` is `protected init`, so `new Agenda { Id = ... }` is valid inside `Agenda` (same as `User`).
   > `Email` is created twice (validate + apply) — same accepted trade-off as `User`.
   > Max-length validation (`Code` 50, `FullName` 200, phones 50, `ImagePath` 300) is optional: without it, a
   > too-long value yields a `DbUpdateException` (→ 500). Recommended to add it here since phone fields are free
   > text (`AgendaErrors.FieldTooLong`), but keep it out if review prefers parity with the other slices.

3. **`Errors/AgendaErrors.cs`** — add the missing errors (keep the existing two), XML summary like `UserErrors`:
   - `AgendaNotFound` → `"AgendaErrors.AgendaNotFound"`, `"The agenda was not found."`
   - `AgendasNotFound` → `"AgendaErrors.AgendasNotFound"`, `"The agendas were not found."`
   - `WorkstationIdCannotBeEmpty`, `LocationIdCannotBeEmpty`, `UserIdCreatedCannotBeEmpty`
   - (optional) `FieldTooLong`
4. **`Services/IAgendaService.cs`** (new) — `public interface IAgendaService : ICrudService<Agenda, Guid>;`

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application/Agendas`)

Copy the Users structure 1:1 (handlers are auto-registered by assembly scan in `DependencyInjection.AddApplication`):

```
Agendas/
├── Services/AgendaService.cs                           // : CrudCqrsService<Agenda, Guid>, IAgendaService
├── Mappers/AgendaCommandMapper.cs                      // ToAgendaCreationData(this CreateAgendaCommand / UpdateAgendaCommand)
└── UseCases/
    ├── Commands/
    │   ├── CreateAgenda/CreateAgendaCommand.cs          // (string Code, string FullName, string Email, bool Enabled,
    │   │                                                //  string? ImagePath, string? DirectLine, string? Internal,
    │   │                                                //  string? Corporate, string? DirectAccess,
    │   │                                                //  Guid WorkstationId, Guid LocationId, Guid UserIdCreated) : ICommand<Result<Agenda>>
    │   ├── CreateAgenda/CreateAgendaCommandHandler.cs
    │   ├── UpdateAgenda/UpdateAgendaCommand.cs          // (Guid Id, ...same fields...)
    │   ├── UpdateAgenda/UpdateAgendaCommandHandler.cs
    │   ├── DeleteAgenda/DeleteAgendaCommand.cs          // (Guid Id) : ICommand<Result>
    │   └── DeleteAgenda/DeleteAgendaCommandHandler.cs
    └── Queries/
        ├── GetAgendaAll/GetAgendaAllQuery.cs            // : IQuery<Result<ICollection<Agenda>>>
        ├── GetAgendaAll/GetAgendaAllQueryHandler.cs
        ├── GetAgendaById/GetAgendaByIdQuery.cs          // (Guid Id) : IQuery<Result<Agenda>>
        └── GetAgendaById/GetAgendaByIdQueryHandler.cs
```

- Handlers inject `IAgendaRepository` + `IUnitOfWork`; same flow as the User handlers
  (`Create`/`Update` validation → repo `Add`/`Update`/`Remove` → `SaveChangesAsync` → `Error.CannotSaveChanges` on 0 rows).
- Create / Update handlers also inject `IWorkstationRepository`, `ILocationRepository` and (Create only)
  `IUserRepository`, and check each parent **before** saving to avoid FK violations (→ 500):
  - `WorkstationErrors.WorkstationNotFound`
  - `LocationErrors.LocationNotFound`
  - `UserErrors.UserNotFound` (Create only — `UserIdCreated` is immutable, so Update skips it)
- `UpdateAgendaCommand` still carries `UserIdCreated` (shared mapper / DTO) but `Agenda.Update` ignores it.
- No `IDateTimeProvider` needed (Agenda has no timestamp column).
- Not-found paths return `AgendaErrors.AgendaNotFound` / `AgendasNotFound`.
- `public sealed class` handlers, XML doc comment style as the User use cases.
- Register in `Application/DependencyInjection.cs` (after `IUserService`):

```csharp
services.AddScoped<IAgendaService, AgendaService>();
```

```csharp
public sealed class CreateAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IWorkstationRepository workstationRepository,
    ILocationRepository locationRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateAgendaCommand, Result<Agenda>>
{
    public async Task<Result<Agenda>> HandleAsync(
        CreateAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agendaResult = Agenda.Create(command.ToAgendaCreationData());
        if (agendaResult.IsFailure)
            return Result.Failure<Agenda>(agendaResult.Error);

        var workstation = await workstationRepository.GetByIdAsync(command.WorkstationId, cancellationToken);
        if (workstation is null)
            return Result.Failure<Agenda>(WorkstationErrors.WorkstationNotFound);

        var location = await locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure<Agenda>(LocationErrors.LocationNotFound);

        var user = await userRepository.GetByIdAsync(command.UserIdCreated, cancellationToken);
        if (user is null)
            return Result.Failure<Agenda>(UserErrors.UserNotFound);

        agendaRepository.Add(agendaResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(agendaResult.Value)
            : Result.Failure<Agenda>(Error.CannotSaveChanges);
    }
}
```

```csharp
public class AgendaService(IDispatcher dispatcher)
    : CrudCqrsService<Agenda, Guid>(dispatcher), IAgendaService
{
    protected override ICommand<Result<Agenda>> BuildCreateCommand(Agenda entity)
    {
        return new CreateAgendaCommand(
            entity.Code,
            entity.FullName,
            entity.Email!.Value,
            entity.Enabled,
            entity.ImagePath,
            entity.DirectLine,
            entity.Internal,
            entity.Corporate,
            entity.DirectAccess,
            entity.WorkstationId,
            entity.LocationId,
            entity.UserIdCreated);
    }

    protected override IQuery<Result<Agenda>> BuildGetByIdQuery(Guid id)
    {
        return new GetAgendaByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<Agenda>>> BuildGetAllQuery()
    {
        return new GetAgendaAllQuery();
    }

    protected override ICommand<Result<Agenda>> BuildUpdateCommand(Guid id, Agenda entity)
    {
        return new UpdateAgendaCommand(id, /* same fields as BuildCreateCommand */);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteAgendaCommand(id);
    }
}
```

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Agendas`)

```
Agendas/
├── Dtos/AgendaDto.cs                 // record (Guid Id, string Code, string FullName, string Email, bool Enabled,
│                                     //         string? ImagePath, string? DirectLine, string? Internal, string? Corporate,
│                                     //         string? DirectAccess, Guid WorkstationId, Guid LocationId, Guid UserIdCreated)
├── Dtos/CreateUpdateAgendaDto.cs     // record (string Code, string FullName, string Email, bool Enabled,
│                                     //         string? ImagePath, string? DirectLine, string? Internal, string? Corporate,
│                                     //         string? DirectAccess, Guid WorkstationId, Guid LocationId, Guid UserIdCreated)
├── Mappers/AgendaMap.cs              // ToDto(Agenda), ToDto(IEnumerable<Agenda>), ToEntity(CreateUpdateAgendaDto) → Result<Agenda>
└── Endpoints/AgendaEndpoints.cs      // MapAgendaEndpoints(this IEndpointRouteBuilder app)
```

- Flat DTO; only the FK ids — `Workstation` / `Location` / `UserCreated` navigations are not loaded by
  `CrudRepository` and are not embedded.
- `Enabled` is a plain `bool` in the request; JSON omission → `false` (see Notes).
- No `IDateTimeProvider` in the endpoints (unlike Users).

Register in `WebApi/Extensions/EndpointsBuilderExtensions.cs` (after `MapUserEndpoints()`):

```csharp
endpoints.MapAgendaEndpoints();
```

Endpoints — same shape as `UserEndpoints`:

```csharp
public static class AgendaEndpoints
{
    public static void MapAgendaEndpoints(this IEndpointRouteBuilder app)
    {
        var agendaGroup = app.MapGroup("/agendas");
        agendaGroup.MapGet("/", GetAgendaAll);
        agendaGroup.MapGet("/{id:guid}", GetAgendaById);
        agendaGroup.MapPost("/", CreateAgenda);
        agendaGroup.MapPut("/{id:guid}", UpdateAgenda);
        agendaGroup.MapDelete("/{id:guid}", DeleteAgenda);
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

    // GetAgendaAll / GetAgendaById / UpdateAgenda / DeleteAgenda: same as UserEndpoints
}
```

Mapper — return the `Result` so invalid input becomes a 400 instead of a 500:

```csharp
public static Result<Agenda> ToEntity(this CreateUpdateAgendaDto createUpdateAgendaDto)
{
    return Agenda.Create(new AgendaCreationData
    {
        Code = createUpdateAgendaDto.Code,
        FullName = createUpdateAgendaDto.FullName,
        Email = createUpdateAgendaDto.Email,
        Enabled = createUpdateAgendaDto.Enabled,
        ImagePath = createUpdateAgendaDto.ImagePath,
        DirectLine = createUpdateAgendaDto.DirectLine,
        Internal = createUpdateAgendaDto.Internal,
        Corporate = createUpdateAgendaDto.Corporate,
        DirectAccess = createUpdateAgendaDto.DirectAccess,
        WorkstationId = createUpdateAgendaDto.WorkstationId,
        LocationId = createUpdateAgendaDto.LocationId,
        UserIdCreated = createUpdateAgendaDto.UserIdCreated
    });
}
```

### Step 4 — Verify

- `dotnet build` the solution.
- Run the WebApi (Development → migrations auto-applied) and exercise with OpenAPI / a `.http` file:
  create work area → workstation → location → profile → user → create agenda with their ids → get by id →
  get all → update (incl. changing workstation / location / phones / `enabled`) → delete → get by id (404).
- Two consecutive creates succeed with **different** ids (regression for the `Guid.Empty` bug).
- `userIdCreated` does not change on update even if a different id is sent.
- Blank phone fields (`""`, `"  "`) are returned as `null`.
- Negative cases → 400: empty `code` / `fullName`; missing / invalid `email`; `workstationId` / `locationId` /
  `userIdCreated` = `00000000-…`; ids not present in their tables (`WorkstationNotFound`, `LocationNotFound`,
  `UserNotFound` — not 500).
- Check `dotnet ef migrations add` produces **no changes** (`Guid?` → `Guid` on already-required FKs, setter
  visibility / ctor change). If a model diff appears, discard it and investigate before committing.

## Scope

**In scope**
- Domain: `AgendaCreationData` → primitives + FK ids, `Agenda` setter/ctor refactor, `Guid?` → `Guid` FKs,
  `Validate`, new `Update`, `CreateVersion7` id, required `Email`, extended `AgendaErrors`, `IAgendaService`.
- Application: 3 commands + 2 queries with handlers (workstation / location / user existence checks),
  `AgendaCommandMapper`, `AgendaService`, DI registration.
- WebApi: `AgendaDto`, `CreateUpdateAgendaDto`, `AgendaMap`, `AgendaEndpoints`, endpoint registration.

**Out of scope**
- Image upload / storage (`ImagePath` is just a string).
- Authentication — `UserIdCreated` from the token instead of the body.
- Search / filtering / pagination on `GET /agendas` (by name, location, workstation, enabled…) — the main phone book
  use case, natural follow-up ticket.
- Embedding `Workstation` / `Location` / `UserCreated` in the payload (only ids).
- Unique index on `code`.
- Optimistic concurrency exposure (`xmin` / `If-Match`).
- PATCH / enable-disable endpoint.
- Unit / integration tests (no test projects exist yet under `test/`).

## Notes / Future Considerations

- **`UserIdCreated` from the body** is a stopgap: anyone can claim any creator. Once auth exists, take it from the
  current user claim and drop it from `CreateUpdateAgendaDto`. It is intentionally immutable on update.
- **Error mapping**: `UpdateAgenda` (following `UserEndpoints`) maps every service failure to 404, so a PUT with an
  unknown `workstationId` / `locationId` returns 404. Same open item as MNPB-120 / 122 / 124 — map by error code later.
- **Delete of parents**: all three FKs are `NoAction`, so deleting a workstation, location or user that is
  referenced by an agenda now becomes reachable and throws `DbUpdateException` (→ 500). The shared `...InUse` → 409
  fix (check `agendaRepository` before delete) becomes more relevant after this ticket.
- **`Enabled` default**: omitting `enabled` in the JSON yields `false`. Consider `bool? Enabled` defaulting to
  `true` in the mapper if clients expect new contacts to be visible by default.
- **Concurrency**: `xmin` is configured as a concurrency token; a concurrent update raises
  `DbUpdateConcurrencyException` (→ 500). Map to 409 together with the error-mapping follow-up.
- **Domain event**: `AgendaCreatedDomainEvent` has no handler yet; harmless (zero handlers), now carries a real id.
- **Email error code**: `Email.Create` returns `UserErrors.InvalidEmail` / `Error.NullValue` even for agendas —
  acceptable, but consider moving `InvalidEmail` to a shared error class.
- `GET /agendas` returns 404 on an empty table (same as other resources); 200 with `[]` is arguably more RESTful.
- Suggested commit split mirroring previous tickets: Domain+Application, then WebApi.
