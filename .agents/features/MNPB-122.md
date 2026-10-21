# MNPB-122 WebApi - Implements Crud Workstation enpoints

## Summary

Expose CRUD HTTP endpoints for `Workstation` under `/workstations` in `MiasoftNanus.PhoneBook.WebApi.Workstations`,
mirroring the WorkArea slice delivered in MNPB-120
(`WebApi/Workstations/{Dtos,Endpoints,Mappers}` → `IWorkstationService` → CQRS use cases → `IWorkstationRepository`).

| Verb   | Route                     | Service call                     | Success                                   | Failure           |
|--------|---------------------------|----------------------------------|-------------------------------------------|-------------------|
| GET    | `/workstations`           | `GetAllAsync`                    | 200 `WorkstationDto[]`                    | 404 `Error`       |
| GET    | `/workstations/{id:guid}` | `GetByIdAsync(id)`               | 200 `WorkstationDto`                      | 404 `Error`       |
| POST   | `/workstations`           | `CreateAsync(workstation)`       | 201 `WorkstationDto` + `Location` header  | 400 `Error`       |
| PUT    | `/workstations/{id:guid}` | `UpdateAsync(id, workstation)`   | 200 `WorkstationDto`                      | 400 / 404 `Error` |
| DELETE | `/workstations/{id:guid}` | `DeleteAsync(id)`                | 204                                       | 404 `Error`       |

## Motivation

Workstations are referenced by `Agenda.WorkstationId`, so clients need to manage them before agendas can be created
through the API. Now that WorkArea CRUD exists (MNPB-120), workstations can be attached to an existing work area.
The repository (`WorkstationRepository`) and EF configuration (`WorkstationConfiguration`, table `workstations`)
already exist and are registered in `Infrastructure/DependencyInjection.cs`, but nothing above the repository does yet.

**Gap analysis vs. WorkAreas** — the WebApi layer cannot be built alone; these pieces are missing:

| Layer          | WorkAreas (exists)                                                                     | Workstations (status)                  |
|----------------|----------------------------------------------------------------------------------------|----------------------------------------|
| Domain entity  | private ctor `(id, code, name, description)`, `private set`, `Create` + `Update` + shared `Validate`, `Guid.CreateVersion7()` | `required init` props, parameterless ctor, `Create` only, `Guid.NewGuid()` → **no `Update`**, **`Create` never sets `WorkAreaId`** |
| Domain errors  | `WorkAreaNotFound`, `WorkAreasNotFound` + validation errors                            | validation errors only → **missing** `WorkstationNotFound`, `WorkstationsNotFound`, `WorkAreaIdCannotBeEmpty` |
| Domain service | `IWorkAreaService : ICrudService<WorkArea, Guid>`                                      | **missing** `IWorkstationService`      |
| Repository     | ✅ `IWorkAreaRepository` / `WorkAreaRepository` (registered)                           | ✅ `IWorkstationRepository` / `WorkstationRepository` (registered) |
| Application    | 5 use cases + `WorkAreaService` + DI registration                                      | **missing** all                        |
| WebApi         | Dtos, Mapper, Endpoints, `MapWorkAreaEndpoints()`                                      | **missing** all                        |

Shape difference vs. WorkArea: `Workstation` has the same three fields — `Code` (required, max 50), `Name`
(required, max 100), `Description` (optional / nullable, max 500) — **plus a required foreign key `WorkAreaId`**
(`work_area_id`, FK to `work_areas`, `DeleteBehavior.NoAction`), a `WorkArea` navigation and an `Agendas`
navigation.

**FK nullability mismatch**: the entity declares `Guid? WorkAreaId`, but `WorkstationConfiguration` marks it
`.IsRequired()` and the model snapshot already has `b.Property<Guid>("WorkAreaId")` (non-nullable). Changing the
entity to `Guid WorkAreaId` aligns code with the existing schema — no migration expected.

## Implementation

Work bottom-up, one commit per layer (builds green after each step).

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Workstations`)

1. **`Entities/Workstation.cs`** — refactor to the same shape as `WorkArea`:
   - Replace the parameterless ctor with `private Workstation(Guid id, string code, string name, string? description, Guid workAreaId)`
     (EF binds ctor params by name, as it already does for `WorkArea` / `Location`).
   - Replace `required ... { get; init; }` on `Code` / `Name` / `Description` with `{ get; private set; }`.
   - Change `Guid? WorkAreaId { get; init; }` → `Guid WorkAreaId { get; private set; }` (matches the schema).
   - Keep `WorkArea? WorkArea { get; init; }` and `ICollection<Agenda>? Agendas { get; }` as navigations.
   - Add `workAreaId` to `Create` and to a new `Result Update(string code, string name, string? description, Guid workAreaId)`.
   - Extract a private static `Validate(code, name, workAreaId)` used by both (adds the `Guid.Empty` check).
   - Switch `Guid.NewGuid()` → `Guid.CreateVersion7()`.
2. **`Errors/WorkstationErrors.cs`** — add (same style as `WorkAreaErrors`, add the XML class summary too):
   - `WorkstationNotFound` → `"WorkstationErrors.WorkstationNotFound"`, `"The workstation was not found."` (before validation errors)
   - `WorkstationsNotFound` → `"WorkstationErrors.WorkstationsNotFound"`, `"The workstations were not found."`
   - `WorkAreaIdCannotBeEmpty` → `"WorkstationErrors.WorkAreaIdCannotBeEmpty"`, `"Work area id cannot be empty."` (after the existing validation errors)
3. **`Services/IWorkstationService.cs`** (new) — `public interface IWorkstationService : ICrudService<Workstation, Guid>;`

```csharp
public class Workstation : Entity
{
    private Workstation(Guid id, string code, string name, string? description, Guid workAreaId)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        WorkAreaId = workAreaId;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid WorkAreaId { get; private set; }
    public WorkArea? WorkArea { get; init; }
    public ICollection<Agenda>? Agendas { get; }

    public static Result<Workstation> Create(string code, string name, string? description, Guid workAreaId)
    {
        var validationResult = Validate(code, name, workAreaId);
        return validationResult.IsFailure
            ? Result.Failure<Workstation>(validationResult.Error)
            : Result.Success(new Workstation(Guid.CreateVersion7(), code, name, description, workAreaId));
    }

    public Result Update(string code, string name, string? description, Guid workAreaId)
    {
        var validationResult = Validate(code, name, workAreaId);
        if (validationResult.IsFailure)
            return validationResult;

        Code = code;
        Name = name;
        Description = description;
        WorkAreaId = workAreaId;
        return Result.Success();
    }

    private static Result Validate(string code, string name, Guid workAreaId)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(WorkstationErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(WorkstationErrors.NameCannotBeNullOrWhitespace);

        if (workAreaId == Guid.Empty)
            return Result.Failure(WorkstationErrors.WorkAreaIdCannotBeEmpty);

        return Result.Success();
    }
}
```

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application/Workstations`)

Copy the WorkArea structure 1:1 (handlers are auto-registered by assembly scan in `DependencyInjection.AddApplication`):

```
Workstations/
├── Services/WorkstationService.cs                            // : CrudCqrsService<Workstation, Guid>, IWorkstationService
└── UseCases/
    ├── Commands/
    │   ├── CreateWorkstation/CreateWorkstationCommand.cs      // (string Code, string Name, string? Description, Guid WorkAreaId) : ICommand<Result<Workstation>>
    │   ├── CreateWorkstation/CreateWorkstationCommandHandler.cs
    │   ├── UpdateWorkstation/UpdateWorkstationCommand.cs      // (Guid Id, string Code, string Name, string? Description, Guid WorkAreaId)
    │   ├── UpdateWorkstation/UpdateWorkstationCommandHandler.cs
    │   ├── DeleteWorkstation/DeleteWorkstationCommand.cs      // (Guid Id) : ICommand<Result>
    │   └── DeleteWorkstation/DeleteWorkstationCommandHandler.cs
    └── Queries/
        ├── GetWorkstationAll/GetWorkstationAllQuery.cs        // : IQuery<Result<ICollection<Workstation>>>
        ├── GetWorkstationAll/GetWorkstationAllQueryHandler.cs
        ├── GetWorkstationById/GetWorkstationByIdQuery.cs      // (Guid Id) : IQuery<Result<Workstation>>
        └── GetWorkstationById/GetWorkstationByIdQueryHandler.cs
```

- Handlers inject `IWorkstationRepository` + `IUnitOfWork`; same flow as WorkArea handlers
  (`Create`/`Update` validation → repo `Add`/`Update`/`Remove` → `SaveChangesAsync` → `Error.CannotSaveChanges` on 0 rows).
- **New vs. WorkArea**: `CreateWorkstationCommandHandler` and `UpdateWorkstationCommandHandler` also inject
  `IWorkAreaRepository` and check the referenced work area exists **before** saving, returning
  `WorkAreaErrors.WorkAreaNotFound` otherwise. Without this, an unknown `workAreaId` violates
  `fk_workstations_work_areas_work_area_id` → `DbUpdateException` → 500.
- Not-found paths return `WorkstationErrors.WorkstationNotFound` / `WorkstationsNotFound`.
- Keep the same XML doc comment style as the WorkArea use cases.
- Register in `Application/DependencyInjection.cs` (after `IWorkAreaService`):

```csharp
services.AddScoped<IWorkstationService, WorkstationService>();
```

```csharp
public sealed class CreateWorkstationCommandHandler(
    IWorkstationRepository workstationRepository,
    IWorkAreaRepository workAreaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateWorkstationCommand, Result<Workstation>>
{
    public async Task<Result<Workstation>> HandleAsync(
        CreateWorkstationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workstationResult = Workstation.Create(command.Code, command.Name, command.Description, command.WorkAreaId);
        if (workstationResult.IsFailure)
            return Result.Failure<Workstation>(workstationResult.Error);

        var workArea = await workAreaRepository.GetByIdAsync(command.WorkAreaId, cancellationToken);
        if (workArea is null)
            return Result.Failure<Workstation>(WorkAreaErrors.WorkAreaNotFound);

        workstationRepository.Add(workstationResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(workstationResult.Value)
            : Result.Failure<Workstation>(Error.CannotSaveChanges);
    }
}
```

```csharp
public class WorkstationService(IDispatcher dispatcher)
    : CrudCqrsService<Workstation, Guid>(dispatcher), IWorkstationService
{
    protected override ICommand<Result<Workstation>> BuildCreateCommand(Workstation entity)
    {
        return new CreateWorkstationCommand(entity.Code, entity.Name, entity.Description, entity.WorkAreaId);
    }

    protected override IQuery<Result<Workstation>> BuildGetByIdQuery(Guid id)
    {
        return new GetWorkstationByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<Workstation>>> BuildGetAllQuery()
    {
        return new GetWorkstationAllQuery();
    }

    protected override ICommand<Result<Workstation>> BuildUpdateCommand(Guid id, Workstation entity)
    {
        return new UpdateWorkstationCommand(id, entity.Code, entity.Name, entity.Description, entity.WorkAreaId);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteWorkstationCommand(id);
    }
}
```

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Workstations`)

```
Workstations/
├── Dtos/WorkstationDto.cs                 // record (Guid Id, string Code, string Name, string? Description, Guid WorkAreaId)
├── Dtos/CreateUpdateWorkstationDto.cs     // record (string Code, string Name, string? Description, Guid WorkAreaId)
├── Mappers/WorkstationMap.cs              // ToDto(Workstation), ToDto(IEnumerable<Workstation>), ToEntity(CreateUpdateWorkstationDto) → Result<Workstation>
└── Endpoints/WorkstationEndpoints.cs      // MapWorkstationEndpoints(this IEndpointRouteBuilder app)
```

Register in `WebApi/Extensions/EndpointsBuilderExtensions.cs` (after `MapWorkAreaEndpoints()`):

```csharp
endpoints.MapWorkstationEndpoints();
```

Endpoints — same shape as `WorkAreaEndpoints`, always returning DTOs (never the entity, because of the
`WorkArea` / `Agendas` navigations):

```csharp
public static class WorkstationEndpoints
{
    public static void MapWorkstationEndpoints(this IEndpointRouteBuilder app)
    {
        var workstationGroup = app.MapGroup("/workstations");
        workstationGroup.MapGet("/", GetWorkstationAll);
        workstationGroup.MapGet("/{id:guid}", GetWorkstationById);
        workstationGroup.MapPost("/", CreateWorkstation);
        workstationGroup.MapPut("/{id:guid}", UpdateWorkstation);
        workstationGroup.MapDelete("/{id:guid}", DeleteWorkstation);
    }

    private static async Task<IResult> CreateWorkstation(
        CreateUpdateWorkstationDto createUpdateWorkstationDto,
        IWorkstationService workstationService,
        CancellationToken cancellationToken)
    {
        var workstationResult = createUpdateWorkstationDto.ToEntity();
        if (workstationResult.IsFailure)
            return TypedResults.BadRequest(workstationResult.Error);

        var result = await workstationService.CreateAsync(workstationResult.Value, cancellationToken);
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Created($"/workstations/{result.Value.Id}", result.Value.ToDto());
    }

    // GetWorkstationAll / GetWorkstationById / UpdateWorkstation / DeleteWorkstation: same as WorkAreaEndpoints
}
```

Mapper — return the `Result` (not `.Value`) so invalid input becomes a 400 instead of a 500:

```csharp
public static Result<Workstation> ToEntity(this CreateUpdateWorkstationDto createUpdateWorkstationDto)
{
    return Workstation.Create(
        createUpdateWorkstationDto.Code,
        createUpdateWorkstationDto.Name,
        createUpdateWorkstationDto.Description,
        createUpdateWorkstationDto.WorkAreaId
    );
}
```

### Step 4 — Verify

- `dotnet build` the solution.
- Run the WebApi (Development → migrations auto-applied) and exercise with OpenAPI / a `.http` file:
  create a work area → create workstation with its id → get by id → get all → update (incl. moving to another
  work area) → delete → get by id (404).
- Negative cases: empty `code` → 400; empty `name` → 400; `workAreaId` = `00000000-…` → 400;
  `workAreaId` not in `work_areas` → 400 `WorkAreaErrors.WorkAreaNotFound` (not 500); `description: null` → 201/200.
- Check `dotnet ef migrations add` produces **no changes** (setter visibility, ctor and `Guid?` → `Guid` only; the
  snapshot already has a non-nullable `WorkAreaId`). If a model diff appears, discard it and investigate before committing.

## Scope

**In scope**
- Domain: `Workstation` ctor/setter refactor, `WorkAreaId` → non-nullable, `workAreaId` in `Create`, new `Update`,
  `Validate`, `CreateVersion7`, `WorkstationNotFound` / `WorkstationsNotFound` / `WorkAreaIdCannotBeEmpty`,
  `IWorkstationService`.
- Application: 3 commands + 2 queries with handlers (with work-area existence check), `WorkstationService`,
  DI registration.
- WebApi: `WorkstationDto`, `CreateUpdateWorkstationDto`, `WorkstationMap`, `WorkstationEndpoints`, endpoint registration.

**Out of scope**
- Embedding the `WorkArea` object or `Agendas` in the Workstation payload (only `WorkAreaId` is exposed).
- Nested routes such as `GET /work-areas/{id}/workstations`.
- Agenda CRUD endpoints (follow-up ticket).
- Authentication / authorization on the endpoints.
- Pagination / filtering on `GET /workstations`.
- Unit / integration tests (no test projects exist yet under `test/`).

## Notes / Future Considerations

- **Error code for missing work area**: returned from the service, so `CreateWorkstation` maps it to 400, but
  `UpdateWorkstation` (following `WorkAreaEndpoints`) maps every service failure to 404 — a PUT with an unknown
  `workAreaId` gives 404, indistinguishable from an unknown workstation id. Consider mapping by error code later
  (same open item as MNPB-120).
- **Delete with agendas**: `AgendaConfiguration` points at `workstations`; deleting a workstation referenced by an
  agenda will throw `DbUpdateException` (→ 500). Same open issue as Location / WorkArea — consider a shared
  `...InUse` → 409 fix in a follow-up.
- **Unique `Code`**: no unique index exists on `workstations.code`. Decide whether code is unique globally or per
  work area (`HasIndex(w => new { w.WorkAreaId, w.Code }).IsUnique()`) in a follow-up (needs a migration).
- **Empty `Description`**: stored as sent; `""` is not normalized to `null` (consistent with WorkArea).
- `GET /workstations` returns 404 on an empty table (same as other resources); 200 with `[]` is arguably more RESTful.
- `Agenda` still uses `required init` props / `Guid? WorkstationId`; it will need the same refactor when its CRUD is implemented.
- Suggested commit split mirroring previous tickets: Domain+Application, then WebApi.
