# MNPB-120 WebApi - Implements Crud WorkArea enpoints

## Summary

Expose CRUD HTTP endpoints for `WorkArea` under `/work-areas` in `MiasoftNanus.PhoneBook.WebApi.WorkAreas`,
mirroring the Location slice delivered in MNPB-116
(`WebApi/WorkAreas/{Dtos,Endpoints,Mappers}` → `IWorkAreaService` → CQRS use cases → `IWorkAreaRepository`).

| Verb   | Route                   | Service call                  | Success                                 | Failure      |
|--------|-------------------------|-------------------------------|-----------------------------------------|--------------|
| GET    | `/work-areas`           | `GetAllAsync`                 | 200 `WorkAreaDto[]`                     | 404 `Error`  |
| GET    | `/work-areas/{id:guid}` | `GetByIdAsync(id)`            | 200 `WorkAreaDto`                       | 404 `Error`  |
| POST   | `/work-areas`           | `CreateAsync(workArea)`       | 201 `WorkAreaDto` + `Location` header   | 400 `Error`  |
| PUT    | `/work-areas/{id:guid}` | `UpdateAsync(id, workArea)`   | 200 `WorkAreaDto`                       | 400 / 404 `Error` |
| DELETE | `/work-areas/{id:guid}` | `DeleteAsync(id)`             | 204                                     | 404 `Error`  |

## Motivation

Work areas are referenced by `Workstation.WorkAreaId`, so clients need a way to manage them before workstations
can be created through the API. The repository (`WorkAreaRepository`) and EF configuration
(`WorkAreaConfiguration`, table `work_areas`) already exist and are registered in
`Infrastructure/DependencyInjection.cs`, but nothing above the repository does yet.

**Gap analysis vs. Locations** — the WebApi layer cannot be built alone; these pieces are missing:

| Layer          | Locations (exists)                                              | WorkAreas (status)                     |
|----------------|-----------------------------------------------------------------|----------------------------------------|
| Domain entity  | private ctor `(id, code, description)`, `private set`, `Create` + `Update` + shared `Validate`, `Guid.CreateVersion7()` | `required init` props, parameterless ctor, `Create` only, `Guid.NewGuid()` → **no `Update`** |
| Domain errors  | `LocationNotFound`, `LocationsNotFound` + validation errors     | validation errors only → **missing** `WorkAreaNotFound`, `WorkAreasNotFound` |
| Domain service | `ILocationService : ICrudService<Location, Guid>`               | **missing** `IWorkAreaService`         |
| Repository     | `ILocationRepository` / `LocationRepository` (registered)       | ✅ `IWorkAreaRepository` / `WorkAreaRepository` (registered) |
| Application    | 5 use cases + `LocationService` + DI registration               | **missing** all                        |
| WebApi         | Dtos, Mapper, Endpoints, `MapLocationEndpoints()`               | **missing** all                        |

Shape difference vs. Location: `WorkArea` has **three** fields — `Code` (required, max 50), `Name` (required,
max 100) and `Description` (**optional / nullable**, max 500) — and a `Workstations` navigation.

## Implementation

Work bottom-up, one commit per layer (builds green after each step).

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/WorkAreas`)

1. **`Entities/WorkArea.cs`** — refactor to the same shape as `Location`:
   - Replace the parameterless ctor with `private WorkArea(Guid id, string code, string name, string? description)`
     (EF binds ctor params by name, as it already does for `Location`).
   - Replace `required ... { get; init; }` on `Code` / `Name` / `Description` with `{ get; private set; }`
     (`required` cannot be combined with a `private` setter — CS9032). Keep `Workstations` as
     `ICollection<Workstation>? { get; init; }` navigation.
   - Extract a private static `Validate(code, name)` used by both `Create` and `Update`.
   - Add `Result Update(string code, string name, string? description)`.
   - Switch `Guid.NewGuid()` → `Guid.CreateVersion7()` for consistency with `Location` / `Profile`.
2. **`Errors/WorkAreaErrors.cs`** — add (same style as `LocationErrors`, placed before the validation errors):
   - `WorkAreaNotFound` → `"WorkAreaErrors.WorkAreaNotFound"`, `"The work area was not found."`
   - `WorkAreasNotFound` → `"WorkAreaErrors.WorkAreasNotFound"`, `"The work areas were not found."`
3. **`Services/IWorkAreaService.cs`** (new) — `public interface IWorkAreaService : ICrudService<WorkArea, Guid>;`

```csharp
public class WorkArea : Entity
{
    private WorkArea(Guid id, string code, string name, string? description)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public ICollection<Workstation>? Workstations { get; init; }

    public static Result<WorkArea> Create(string code, string name, string? description)
    {
        var validationResult = Validate(code, name);
        return validationResult.IsFailure
            ? Result.Failure<WorkArea>(validationResult.Error)
            : Result.Success(new WorkArea(Guid.CreateVersion7(), code, name, description));
    }

    public Result Update(string code, string name, string? description)
    {
        var validationResult = Validate(code, name);
        if (validationResult.IsFailure)
            return validationResult;

        Code = code;
        Name = name;
        Description = description;
        return Result.Success();
    }

    private static Result Validate(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(WorkAreaErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(WorkAreaErrors.NameCannotBeNullOrWhitespace);

        return Result.Success();
    }
}
```

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application/WorkAreas`)

Copy the Location structure 1:1 (handlers are auto-registered by assembly scan in `DependencyInjection.AddApplication`):

```
WorkAreas/
├── Services/WorkAreaService.cs                         // : CrudCqrsService<WorkArea, Guid>, IWorkAreaService
└── UseCases/
    ├── Commands/
    │   ├── CreateWorkArea/CreateWorkAreaCommand.cs      // (string Code, string Name, string? Description) : ICommand<Result<WorkArea>>
    │   ├── CreateWorkArea/CreateWorkAreaCommandHandler.cs
    │   ├── UpdateWorkArea/UpdateWorkAreaCommand.cs      // (Guid Id, string Code, string Name, string? Description)
    │   ├── UpdateWorkArea/UpdateWorkAreaCommandHandler.cs
    │   ├── DeleteWorkArea/DeleteWorkAreaCommand.cs      // (Guid Id) : ICommand<Result>
    │   └── DeleteWorkArea/DeleteWorkAreaCommandHandler.cs
    └── Queries/
        ├── GetWorkAreaAll/GetWorkAreaAllQuery.cs        // : IQuery<Result<ICollection<WorkArea>>>
        ├── GetWorkAreaAll/GetWorkAreaAllQueryHandler.cs
        ├── GetWorkAreaById/GetWorkAreaByIdQuery.cs      // (Guid Id) : IQuery<Result<WorkArea>>
        └── GetWorkAreaById/GetWorkAreaByIdQueryHandler.cs
```

- Handlers inject `IWorkAreaRepository` + `IUnitOfWork`; same flow as Location handlers
  (`Create`/`Update` validation → repo `Add`/`Update`/`Remove` → `SaveChangesAsync` → `Error.CannotSaveChanges` on 0 rows).
- Not-found paths return `WorkAreaErrors.WorkAreaNotFound` / `WorkAreasNotFound`.
- Keep the same XML doc comment style as the Location use cases.
- Register in `Application/DependencyInjection.cs` (after `ILocationService`):

```csharp
services.AddScoped<IWorkAreaService, WorkAreaService>();
```

```csharp
public class WorkAreaService(IDispatcher dispatcher) : CrudCqrsService<WorkArea, Guid>(dispatcher), IWorkAreaService
{
    protected override ICommand<Result<WorkArea>> BuildCreateCommand(WorkArea entity)
    {
        return new CreateWorkAreaCommand(entity.Code, entity.Name, entity.Description);
    }

    protected override IQuery<Result<WorkArea>> BuildGetByIdQuery(Guid id)
    {
        return new GetWorkAreaByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<WorkArea>>> BuildGetAllQuery()
    {
        return new GetWorkAreaAllQuery();
    }

    protected override ICommand<Result<WorkArea>> BuildUpdateCommand(Guid id, WorkArea entity)
    {
        return new UpdateWorkAreaCommand(id, entity.Code, entity.Name, entity.Description);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteWorkAreaCommand(id);
    }
}
```

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/WorkAreas`)

```
WorkAreas/
├── Dtos/WorkAreaDto.cs                 // record (Guid Id, string Code, string Name, string? Description)
├── Dtos/CreateUpdateWorkAreaDto.cs     // record (string Code, string Name, string? Description)
├── Mappers/WorkAreaMap.cs              // ToDto(WorkArea), ToDto(IEnumerable<WorkArea>), ToEntity(CreateUpdateWorkAreaDto) → Result<WorkArea>
└── Endpoints/WorkAreaEndpoints.cs      // MapWorkAreaEndpoints(this IEndpointRouteBuilder app)
```

Register in `WebApi/Extensions/EndpointsBuilderExtensions.cs` (after `MapLocationEndpoints()`):

```csharp
endpoints.MapWorkAreaEndpoints();
```

Endpoints (same shape as `LocationEndpoints`, always returning DTOs — never the entity, because of the
`Workstations` navigation):

```csharp
public static class WorkAreaEndpoints
{
    public static void MapWorkAreaEndpoints(this IEndpointRouteBuilder app)
    {
        var workAreaGroup = app.MapGroup("/workareas");
        workAreaGroup.MapGet("/", GetWorkAreaAll);
        workAreaGroup.MapGet("/{id:guid}", GetWorkAreaById);
        workAreaGroup.MapPost("/", CreateWorkArea);
        workAreaGroup.MapPut("/{id:guid}", UpdateWorkArea);
        workAreaGroup.MapDelete("/{id:guid}", DeleteWorkArea);
    }

    private static async Task<IResult> CreateWorkArea(
        CreateUpdateWorkAreaDto createUpdateWorkAreaDto,
        IWorkAreaService workAreaService,
        CancellationToken cancellationToken)
    {
        var workAreaResult = createUpdateWorkAreaDto.ToEntity();
        if (workAreaResult.IsFailure)
            return TypedResults.BadRequest(workAreaResult.Error);

        var result = await workAreaService.CreateAsync(workAreaResult.Value, cancellationToken);
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Created($"/work-areas/{result.Value.Id}", result.Value.ToDto());
    }

    // GetWorkAreaAll / GetWorkAreaById / UpdateWorkArea / DeleteWorkArea: same as LocationEndpoints
}
```

Mapper — return the `Result` (not `.Value`) so invalid input becomes a 400 instead of a 500:

```csharp
public static Result<WorkArea> ToEntity(this CreateUpdateWorkAreaDto createUpdateWorkAreaDto)
{
    return WorkArea.Create(
        createUpdateWorkAreaDto.Code,
        createUpdateWorkAreaDto.Name,
        createUpdateWorkAreaDto.Description);
}
```

### Step 4 — Verify

- `dotnet build` the solution.
- Run the WebApi (Development → migrations auto-applied) and exercise with OpenAPI / a `.http` file:
  create → get by id → get all → update → delete → get by id (404);
  POST with empty `code` → 400; POST with empty `name` → 400; POST/PUT with `description: null` → 201/200.
- Check `dotnet ef migrations add` produces **no changes** (only setter visibility / ctor change; table and columns
  unchanged). If a model diff appears, discard it and investigate before committing.

## Scope

**In scope**
- Domain: `WorkArea.Update`, ctor/setter refactor, `Validate`, `CreateVersion7`, `WorkAreaNotFound` /
  `WorkAreasNotFound`, `IWorkAreaService`.
- Application: 3 commands + 2 queries with handlers, `WorkAreaService`, DI registration.
- WebApi: `WorkAreaDto`, `CreateUpdateWorkAreaDto`, `WorkAreaMap`, `WorkAreaEndpoints`, endpoint registration.

**Out of scope**
- Exposing `Workstations` in the WorkArea payload.
- Workstation CRUD endpoints (follow-up ticket).
- Authentication / authorization on the endpoints.
- Pagination / filtering on `GET /work-areas`.
- Unit / integration tests (no test projects exist yet under `test/`).

## Notes / Future Considerations

- **Route naming**: `/workareas` (matching the C# name) is proposed for a multi-word resource. If the team prefers
  kebab-case, `/work-areas` is the alternative — decide before implementing and keep it consistent in
  `MapGroup` and the `Created` URI.
- **Delete with workstations**: `WorkAreaConfiguration` uses `DeleteBehavior.NoAction` on `Workstations`, so
  deleting a work area referenced by a workstation will throw `DbUpdateException` (→ 500). Same open issue as
  Location/Agenda in MNPB-116 — consider a shared fix (e.g. `WorkAreaErrors.WorkAreaInUse` → 409) in a follow-up.
- **Unique `Code`**: no unique index exists on `work_areas.code`. Consider
  `HasIndex(w => w.Code).IsUnique()` + `IWorkAreaRepository.GetByCodeAsync` + `WorkAreaErrors.CodeAlreadyExists`
  in a follow-up (needs a migration).
- **`UpdateWorkArea` status codes**: following `LocationEndpoints`, DTO validation failures return 400 but
  service failures all map to 404 (including `Error.CannotSaveChanges`). Consider mapping by error code later.
- **Empty `Description`**: `Description` is optional; decide whether `""` should be normalized to `null`
  (not done in this ticket — stored as sent).
- `GET /work-areas` returns 404 on an empty table (same as Locations/Profiles); 200 with `[]` is arguably more RESTful.
- `Workstation` still uses `required init` props and `Guid.NewGuid()`; it will need the same refactor when its
  CRUD is implemented.
- Suggested commit split mirroring previous tickets: Domain+Application, then WebApi.
