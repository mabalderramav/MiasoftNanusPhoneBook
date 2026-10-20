# MNPB-116 WebApi - Implements Crud Location enpoints

## Summary

Expose CRUD HTTP endpoints for `Location` under `/locations` in `MiasoftNanus.PhoneBook.WebApi.Locations`,
mirroring the Profile slice delivered in MNPB-109 / MNPB-111 / MNPB-114
(`WebApi/Profiles/{Dtos,Endpoints,Mappers}` → `ILocationService` → CQRS use cases → `ILocationRepository`).

| Verb   | Route                  | Service call                  | Success          | Failure         |
|--------|------------------------|-------------------------------|------------------|-----------------|
| GET    | `/locations`           | `GetAllAsync`                 | 200 `LocationDto[]` | 404 `Error`  |
| GET    | `/locations/{id:guid}` | `GetByIdAsync(id)`            | 200 `LocationDto`   | 404 `Error`  |
| POST   | `/locations`           | `CreateAsync(location)`       | 201 `LocationDto` + `Location` header | 400 `Error` |
| PUT    | `/locations/{id:guid}` | `UpdateAsync(id, location)`   | 200 `LocationDto`   | 404 `Error`  |
| DELETE | `/locations/{id:guid}` | `DeleteAsync(id)`             | 204              | 404 `Error`     |

## Motivation

Locations are referenced by `Agenda.LocationId`, so clients need a way to manage them before agendas can be
created through the API. The repository (`LocationRepository`) and EF configuration (`LocationConfiguration`)
already exist and are registered in `Infrastructure/DependencyInjection.cs`, but nothing above the
repository does yet.

**Gap analysis vs. Profiles** — the WebApi layer cannot be built alone; these pieces are missing:

| Layer          | Profiles (exists)                                  | Locations (status)                     |
|----------------|----------------------------------------------------|----------------------------------------|
| Domain entity  | `Profile.Create` + `Profile.Update`, `private set` | `Create` only, `required init` props → **no `Update`** |
| Domain errors  | `ProfileNotFound`, `ProfilesNotFound`              | **missing** `LocationNotFound`, `LocationsNotFound` |
| Domain service | `IProfileService : ICrudService<Profile, Guid>`    | **missing** `ILocationService`         |
| Repository     | `IProfileRepository` / `ProfileRepository`         | ✅ `ILocationRepository` / `LocationRepository` (registered) |
| Application    | 5 use cases + `ProfileService` + DI registration   | **missing** all                        |
| WebApi         | Dtos, Mapper, Endpoints, `MapProfileEndpoints()`   | **missing** all                        |

## Implementation

Work bottom-up, one commit per layer (builds green after each step).

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Locations`)

1. **`Entities/Location.cs`** — make it mutable through a domain method, like `Profile`:
   - Replace `required ... { get; init; }` on `Code` / `Description` with `{ get; private set; }`
     (`required` cannot be combined with a `private` setter — CS9032), and use a private constructor
     `Location(Guid id, string code, string description)`. Keep `Agendas` as navigation.
   - Keep a parameterless private ctor only if EF needs it (Profile works without one; EF binds ctor params by name).
   - Add `Result Update(string code, string description)` with the same validations as `Create`.
   - Optional: switch `Guid.NewGuid()` → `Guid.CreateVersion7()` for consistency with `Profile`.
2. **`Errors/LocationErrors.cs`** — add:
   - `LocationNotFound` → `"LocationErrors.LocationNotFound"`, `"The location was not found."`
   - `LocationsNotFound` → `"LocationErrors.LocationsNotFound"`, `"The locations were not found."`
3. **`Services/ILocationService.cs`** (new) — `public interface ILocationService : ICrudService<Location, Guid>;`

```csharp
public Result Update(string code, string description)
{
    if (string.IsNullOrWhiteSpace(code))
        return Result.Failure(LocationErrors.CodeCannotBeNullOrWhitespace);

    if (string.IsNullOrWhiteSpace(description))
        return Result.Failure(LocationErrors.DescriptionCannotBeNullOrWhitespace);

    Code = code;
    Description = description;
    return Result.Success();
}
```

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application/Locations`)

Copy the Profile structure 1:1 (handlers are auto-registered by assembly scan in `DependencyInjection.AddApplication`):

```
Locations/
├── Services/LocationService.cs                         // : CrudCqrsService<Location, Guid>, ILocationService
└── UseCases/
    ├── Commands/
    │   ├── CreateLocation/CreateLocationCommand.cs      // (string Code, string Description) : ICommand<Result<Location>>
    │   ├── CreateLocation/CreateLocationCommandHandler.cs
    │   ├── UpdateLocation/UpdateLocationCommand.cs      // (Guid Id, string Code, string Description)
    │   ├── UpdateLocation/UpdateLocationCommandHandler.cs
    │   ├── DeleteLocation/DeleteLocationCommand.cs      // (Guid Id) : ICommand<Result>
    │   └── DeleteLocation/DeleteLocationCommandHandler.cs
    └── Queries/
        ├── GetLocationAll/GetLocationAllQuery.cs        // : IQuery<Result<ICollection<Location>>>
        ├── GetLocationAll/GetLocationAllQueryHandler.cs
        ├── GetLocationById/GetLocationByIdQuery.cs      // (Guid Id) : IQuery<Result<Location>>
        └── GetLocationById/GetLocationByIdQueryHandler.cs
```

- Handlers inject `ILocationRepository` + `IUnitOfWork`; same flow as Profile handlers
  (`Create`/`Update` validation → repo `Add`/`Update`/`Remove` → `SaveChangesAsync` → `Error.CannotSaveChanges` on 0 rows).
- Not-found paths return `LocationErrors.LocationNotFound` / `LocationsNotFound`.
- Register in `Application/DependencyInjection.cs`:

```csharp
services.AddScoped<ILocationService, LocationService>();
```

```csharp
public class LocationService(IDispatcher dispatcher) : CrudCqrsService<Location, Guid>(dispatcher), ILocationService
{
    protected override ICommand<Result<Location>> BuildCreateCommand(Location entity) =>
        new CreateLocationCommand(entity.Code, entity.Description);

    protected override IQuery<Result<Location>> BuildGetByIdQuery(Guid id) => new GetLocationByIdQuery(id);

    protected override IQuery<Result<ICollection<Location>>> BuildGetAllQuery() => new GetLocationAllQuery();

    protected override ICommand<Result<Location>> BuildUpdateCommand(Guid id, Location entity) =>
        new UpdateLocationCommand(id, entity.Code, entity.Description);

    protected override ICommand<Result> BuildDeleteCommand(Guid id) => new DeleteLocationCommand(id);
}
```

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Locations`)

```
Locations/
├── Dtos/LocationDto.cs                 // record (Guid Id, string Code, string Description)
├── Dtos/CreateUpdateLocationDto.cs     // record (string Code, string Description)
├── Mappers/LocationMap.cs              // ToDto(Location), ToDto(IEnumerable<Location>), ToEntity(CreateUpdateLocationDto)
└── Endpoints/LocationEndpoints.cs      // MapLocationEndpoints(this IEndpointRouteBuilder app)
```

Register in `WebApi/Extensions/EndpointsBuilderExtensions.cs`:

```csharp
endpoints.MapLocationEndpoints();
```

Endpoints (same shape as `ProfileEndpoints`, but always return DTOs — see Notes):

```csharp
public static class LocationEndpoints
{
    public static void MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var locationGroup = app.MapGroup("/locations");
        locationGroup.MapGet("/", GetLocationAll);
        locationGroup.MapGet("/{id:guid}", GetLocationById);
        locationGroup.MapPost("/", CreateLocation);
        locationGroup.MapPut("/{id:guid}", UpdateLocation);
        locationGroup.MapDelete("/{id:guid}", DeleteLocation);
    }

    private static async Task<IResult> CreateLocation(
        CreateUpdateLocationDto createUpdateLocationDto,
        ILocationService locationService,
        CancellationToken cancellationToken)
    {
        var locationResult = createUpdateLocationDto.ToEntity();
        if (locationResult.IsFailure)
            return TypedResults.BadRequest(locationResult.Error);

        var result = await locationService.CreateAsync(locationResult.Value, cancellationToken);
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Created($"/locations/{result.Value.Id}", result.Value.ToDto());
    }

    // GetLocationAll / GetLocationById / UpdateLocation / DeleteLocation: same as ProfileEndpoints
}
```

Mapper — return the `Result` instead of `.Value` so invalid input becomes a 400 instead of an
`InvalidOperationException` (500):

```csharp
public static Result<Location> ToEntity(this CreateUpdateLocationDto dto) =>
    Location.Create(dto.Code, dto.Description);
```

### Step 4 — Verify

- `dotnet build` the solution.
- Run the WebApi (Development → migrations auto-applied) and exercise with OpenAPI / `.http` file:
  create → get by id → get all → update → delete → get by id (404); POST with empty `code` → 400.
- No migration expected: table/columns are unchanged (only setter visibility changes).

## Scope

**In scope**
- Domain: `Location.Update`, setter/ctor refactor, `LocationNotFound` / `LocationsNotFound`, `ILocationService`.
- Application: 3 commands + 2 queries with handlers, `LocationService`, DI registration.
- WebApi: `LocationDto`, `CreateUpdateLocationDto`, `LocationMap`, `LocationEndpoints`, endpoint registration.

**Out of scope**
- Exposing `Agendas` in the Location payload.
- Authentication / authorization on the endpoints.
- Pagination / filtering on `GET /locations`.
- Refactoring the Profile slice (see Notes).

## Notes / Future Considerations

- **Delete with agendas**: `LocationConfiguration` uses `DeleteBehavior.NoAction` on `Agendas`, so deleting a
  location referenced by an agenda will throw `DbUpdateException` (→ 500). Options: check
  `IAgendaRepository` in `DeleteLocationCommandHandler` and return a new `LocationErrors.LocationInUse` (409),
  or defer to a follow-up ticket.
- **Unique `Code`**: no unique index exists on `locations.code`. Consider `HasIndex(l => l.Code).IsUnique()`
  + `ILocationRepository.GetByCodeAsync` + `LocationErrors.CodeAlreadyExists` in a follow-up (needs a migration).
- **Divergences from Profiles (intentional, worth back-porting in a later ticket)**:
  - `ProfileEndpoints.CreateProfile` / `UpdateProfile` return the entity (`result.Value`), not `ProfileDto`.
    For Location the entity has an `Agendas` navigation, so always return `ToDto()`.
  - `ProfileMap.ToEntity` calls `.Value` on a possibly failed `Result` → 500 on invalid input.
  - DTO naming: `CrearUpdateProfileDto` (mixed Spanish/English) vs. proposed `CreateUpdateLocationDto`.
  - `UpdateProfile` maps every failure to 404, including validation errors; consider 400 vs 404 by error code.
- `GET /locations` returns 404 on an empty table (same as Profiles); a 200 with `[]` is arguably more RESTful.
- Suggested commit split / branches mirroring previous tickets: Domain+Application (like MNPB-111), WebApi (like MNPB-114).
