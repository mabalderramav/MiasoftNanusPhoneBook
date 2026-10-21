# MNPB-124 WebApi - Implements Crud User enpoints

## Summary

Expose CRUD HTTP endpoints for `User` under `/users` in `MiasoftNanus.PhoneBook.WebApi.Users`,
mirroring the Workstation slice delivered in MNPB-122
(`WebApi/Users/{Dtos,Endpoints,Mappers}` → `IUserService` → CQRS use cases → `IUserRepository`).

| Verb   | Route              | Service call               | Success                            | Failure           |
|--------|--------------------|----------------------------|------------------------------------|-------------------|
| GET    | `/users`           | `GetAllAsync`              | 200 `UserDto[]`                    | 404 `Error`       |
| GET    | `/users/{id:guid}` | `GetByIdAsync(id)`         | 200 `UserDto`                      | 404 `Error`       |
| POST   | `/users`           | `CreateAsync(user)`        | 201 `UserDto` + `Location` header  | 400 `Error`       |
| PUT    | `/users/{id:guid}` | `UpdateAsync(id, user)`    | 200 `UserDto`                      | 400 / 404 `Error` |
| DELETE | `/users/{id:guid}` | `DeleteAsync(id)`          | 204                                | 404 `Error`       |

`UserDto` **never** contains the password.

## Motivation

Users are referenced by `Agenda.UserIdCreated`, so they must be manageable through the API before agenda CRUD can
be built. The repository (`UserRepository`) and EF configuration (`UserConfiguration`, table `phonebook_users`)
already exist and are registered in `Infrastructure/DependencyInjection.cs`. There is also an **early, unused**
Users application slice (`Application/Users/Commands/CreateUser`, `Application/Users/Queries/GetUserById`) that
predates the `ICrudService` / `CrudCqrsService` pattern and cannot be plugged into it:

- `CreateUserCommand` and `GetUserByIdQuery` are declared `public abstract record` → they can never be instantiated.
- They return `Result<Guid>` / `Result<GetUserByIdResult>` instead of `Result<User>`.
- `CreateUserCommand` resolves the profile by `RoleName`; the other slices reference parents by id (`WorkAreaId`).
- Folder layout is `Users/Commands|Queries`, not `Users/UseCases/Commands|Queries`.
- Handlers are `internal sealed`; the other slices use `public sealed`.

**Gap analysis vs. Workstations**

| Layer          | Workstations (exists)                                                                        | Users (status)                                                    |
|----------------|----------------------------------------------------------------------------------------------|-------------------------------------------------------------------|
| Domain entity  | private ctor, `private set`, `Create` + `Update` + shared `Validate`, `Guid.CreateVersion7()` | `init` props, protected parameterless ctor, `Create(UserCreationData)` only, `Guid.NewGuid()` → **no `Update`**, minimal validation (only names, via `UsernameServices`) |
| Domain data    | n/a                                                                                          | `UserCreationData` holds `Password` / `Email` value objects → the WebApi mapper cannot build it without duplicating VO creation |
| Domain errors  | `WorkstationNotFound`, `WorkstationsNotFound` + validation errors                            | `User.NotFound`, `User.InvalidPassword`, `User.InvalidEmail` → **missing** `UsersNotFound` + validation errors; code style differs (`User.X` vs `UserErrors.X`) |
| Domain service | `IWorkstationService : ICrudService<Workstation, Guid>`                                      | **missing** `IUserService`                                        |
| Repository     | ✅ `IWorkstationRepository` / `WorkstationRepository` (registered)                           | ✅ `IUserRepository` / `UserRepository` (registered)              |
| Application    | 5 use cases + `WorkstationService` + DI registration                                         | legacy, unusable Create/GetById → **replace** with 5 use cases + `UserService` + DI |
| WebApi         | Dtos, Mapper, Endpoints, `MapWorkstationEndpoints()`                                         | **missing** all                                                   |

Shape of `User` (from `UserConfiguration` / snapshot — no schema change planned):

| Property           | Type                  | Column                       | Notes                                                    |
|--------------------|-----------------------|------------------------------|----------------------------------------------------------|
| `FirstName`        | `string`              | `first_name` varchar(50), req |                                                          |
| `LastName`         | `string`              | `last_name` varchar(100), req |                                                          |
| `Password`         | `Password` VO         | `password` text, req         | min 8 chars, **stored in plain text** (see Notes)         |
| `UserName`         | `Username` VO         | `user_name` text, req        | generated: `F.LASTNAME` by `UsernameServices`             |
| `Birthdate`        | `DateTime`            | `birthdate` timestamptz      | must be `DateTimeKind.Utc` for Npgsql                     |
| `Email`            | `Email` VO            | `email` text, req            | regex validated                                           |
| `Address`          | owned `Address` record| `address_country/state/province/district/street` text, req | owned type → EF **cannot** bind it through a ctor |
| `States`           | `States` enum         | `states` text, req           | stored as string (`Active` / `Inactive`)                  |
| `DateOfLastChange` | `DateTime`            | `date_of_last_change` timestamptz | set by `IDateTimeProvider`                           |
| `ProfileId`        | `Guid`                | `profile_id` FK → `phonebook_profiles` (cascade) | parent like `Workstation.WorkAreaId` |

## Implementation

Work bottom-up, one commit per layer (builds green after each step).

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Users`)

1. **`Entities/UserCreationData.cs`** — switch to primitives so the WebApi mapper, the service and the handlers can
   all build it without creating value objects themselves; reused by `Create` and `Update`:

```csharp
public sealed class UserCreationData
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Password { get; init; }
    public required DateTime Birthdate { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public required States States { get; init; }
    public required DateTime DateOfLastChange { get; init; }
    public required Guid ProfileId { get; init; }
}
```

2. **`Entities/User.cs`** — refactor to the Workstation shape:
   - `init` → `private set` on every mapped property (keep the existing property types to avoid nullability churn).
   - Keep a **private** parameterless ctor for EF: `Address` is an owned type and EF cannot bind owned navigations
     through constructor parameters (unlike `Workstation`, which binds via its private ctor).
   - `Create(UserCreationData data)`: run `Validate`, build `Password` / `Email` VOs, generate `Username`,
     `Guid.CreateVersion7()`, raise `UserCreatedDomainEvent` (unchanged).
   - New `Result Update(UserCreationData data)`: same validation; updates every field **except `UserName`**
     (username is generated once at creation — see Notes).
   - Normalize `Birthdate` to UTC date: `DateTime.SpecifyKind(data.Birthdate.Date, DateTimeKind.Utc)`
     (JSON `"1990-05-10"` arrives as `Unspecified`, which Npgsql rejects for `timestamptz`; a birthdate is a date,
     so no timezone shift is wanted — replaces the old `ToUniversalTime()` in `CreateUserCommandMapper`).

```csharp
public class User : Entity
{
    private User()
    {
    }

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public Password? Password { get; private set; }
    public Username? UserName { get; private set; }
    public DateTime Birthdate { get; private set; }
    public Email? Email { get; private set; }
    public Address? Address { get; private set; }
    public States? States { get; private set; }
    public DateTime DateOfLastChange { get; private set; }
    public Guid ProfileId { get; private set; }
    public Profile? Profile { get; init; }
    public ICollection<Agenda>? Agendas { get; init; }

    public static Result<User> Create(UserCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return Result.Failure<User>(validationResult.Error);

        var usernameResult = UsernameServices.GenerateUsername(data.FirstName, data.LastName);
        if (usernameResult.IsFailure)
            return Result.Failure<User>(usernameResult.Error);

        var user = new User { Id = Guid.CreateVersion7(), UserName = usernameResult.Value };
        user.Apply(data);

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return Result.Success(user);
    }

    public Result Update(UserCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return validationResult;

        Apply(data);
        return Result.Success();
    }

    private void Apply(UserCreationData data)
    {
        FirstName = data.FirstName;
        LastName = data.LastName;
        Password = ObjectValues.Password.Create(data.Password).Value;
        Birthdate = DateTime.SpecifyKind(data.Birthdate.Date, DateTimeKind.Utc);
        Email = Shared.ObjectValues.Email.Create(data.Email).Value;
        Address = data.Address;
        States = data.States;
        DateOfLastChange = data.DateOfLastChange;
        ProfileId = data.ProfileId;
    }

    private static Result Validate(UserCreationData data)
    {
        if (string.IsNullOrWhiteSpace(data.FirstName))
            return Result.Failure(UserErrors.FirstNameCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(data.LastName))
            return Result.Failure(UserErrors.LastNameCannotBeNullOrWhitespace);

        var passwordResult = ObjectValues.Password.Create(data.Password);
        if (passwordResult.IsFailure)
            return Result.Failure(passwordResult.Error);

        var emailResult = Shared.ObjectValues.Email.Create(data.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        if (data.Birthdate == default)
            return Result.Failure(UserErrors.BirthdateIsRequired);

        if (data.Address is null
            || string.IsNullOrWhiteSpace(data.Address.Country)
            || string.IsNullOrWhiteSpace(data.Address.State)
            || string.IsNullOrWhiteSpace(data.Address.Province)
            || string.IsNullOrWhiteSpace(data.Address.District)
            || string.IsNullOrWhiteSpace(data.Address.Street))
            return Result.Failure(UserErrors.AddressIsIncomplete);

        if (!Enum.IsDefined(data.States))
            return Result.Failure(UserErrors.InvalidState);

        if (data.ProfileId == Guid.Empty)
            return Result.Failure(UserErrors.ProfileIdCannotBeEmpty);

        return Result.Success();
    }
}
```

   > `Entity.Id` is `protected init`, so the object initializer `new User { Id = ... }` is valid inside `User`.
   > The VOs are created twice (validate + apply); acceptable for clarity — alternatively make `Validate` return the
   > built VOs. Keep whichever reads better in review.

3. **`Errors/UserErrors.cs`** — align codes with the other slices (`UserErrors.X`; nothing consumes the old codes
   yet) and add the missing ones:
   - `UserNotFound` → `"UserErrors.UserNotFound"`, `"The user was not found."`
   - `UsersNotFound` → `"UserErrors.UsersNotFound"`, `"The users were not found."`
   - `FirstNameCannotBeNullOrWhitespace`, `LastNameCannotBeNullOrWhitespace`
   - `InvalidPassword` → `"UserErrors.InvalidPassword"` (kept, used by `Password`)
   - `InvalidEmail` → `"UserErrors.InvalidEmail"` (kept, used by `Email`)
   - `BirthdateIsRequired`, `AddressIsIncomplete`, `InvalidState`, `ProfileIdCannotBeEmpty`
4. **`Services/IUserService.cs`** (new) — `public interface IUserService : ICrudService<User, Guid>;`

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application/Users`)

**Delete** the legacy slice (unused, not instantiable):
`Commands/CreateUser/{CrearUsuarioCommandHandler,CreateUserCommand,CreateUserCommandMapper}.cs`,
`Queries/GetUserById/{GetUserByIdQuery,GetUserByIdQueryHandler,GetUserByIdResult,UserMapper}.cs`.

Then copy the Workstation structure 1:1 (handlers are auto-registered by assembly scan in `DependencyInjection.AddApplication`):

```
Users/
├── Services/UserService.cs                         // : CrudCqrsService<User, Guid>, IUserService
└── UseCases/
    ├── Commands/
    │   ├── CreateUser/CreateUserCommand.cs          // (string FirstName, string LastName, string Password, DateTime Birthdate,
    │   │                                            //  string Email, string Country, string State, string Province,
    │   │                                            //  string District, string Street, States States, Guid ProfileId) : ICommand<Result<User>>
    │   ├── CreateUser/CreateUserCommandHandler.cs
    │   ├── UpdateUser/UpdateUserCommand.cs          // (Guid Id, ...same fields...)
    │   ├── UpdateUser/UpdateUserCommandHandler.cs
    │   ├── DeleteUser/DeleteUserCommand.cs          // (Guid Id) : ICommand<Result>
    │   └── DeleteUser/DeleteUserCommandHandler.cs
    └── Queries/
        ├── GetUserAll/GetUserAllQuery.cs            // : IQuery<Result<ICollection<User>>>
        ├── GetUserAll/GetUserAllQueryHandler.cs
        ├── GetUserById/GetUserByIdQuery.cs          // (Guid Id) : IQuery<Result<User>>
        └── GetUserById/GetUserByIdQueryHandler.cs
```

- Handlers inject `IUserRepository` + `IUnitOfWork`; same flow as Workstation handlers
  (`Create`/`Update` validation → repo `Add`/`Update`/`Remove` → `SaveChangesAsync` → `Error.CannotSaveChanges` on 0 rows).
- Create / Update handlers also inject:
  - `IProfileRepository` → check `GetByIdAsync(command.ProfileId)` **before** saving, return
    `ProfileErrors.ProfileNotFound` otherwise (avoids FK `fk_phonebook_users_phonebook_profiles_profile_id` → 500).
  - `IDateTimeProvider` → `DateOfLastChange = dateTimeProvider.CurrentTime` (the value coming from the
    service/command is ignored; the handler is the source of truth).
- Build `UserCreationData` from the command in a small private/extension helper
  (`UseCases/Commands/UserCommandMapper.cs` → `ToUserCreationData(this CreateUserCommand, DateTime now)` and an
  overload for `UpdateUserCommand`) to keep handlers short.
- Not-found paths return `UserErrors.UserNotFound` / `UsersNotFound`.
- `public sealed class` handlers, XML doc comment style as the Workstation use cases.
- Register in `Application/DependencyInjection.cs` (after `IWorkstationService`):

```csharp
services.AddScoped<IUserService, UserService>();
```

```csharp
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var userResult = User.Create(command.ToUserCreationData(dateTimeProvider.CurrentTime));
        if (userResult.IsFailure)
            return Result.Failure<User>(userResult.Error);

        var profile = await profileRepository.GetByIdAsync(command.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<User>(ProfileErrors.ProfileNotFound);

        userRepository.Add(userResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(userResult.Value)
            : Result.Failure<User>(Error.CannotSaveChanges);
    }
}
```

```csharp
public class UserService(IDispatcher dispatcher)
    : CrudCqrsService<User, Guid>(dispatcher), IUserService
{
    protected override ICommand<Result<User>> BuildCreateCommand(User entity)
    {
        return new CreateUserCommand(
            entity.FirstName!, entity.LastName!, entity.Password!.Value, entity.Birthdate, entity.Email!.Value,
            entity.Address!.Country, entity.Address.State, entity.Address.Province, entity.Address.District,
            entity.Address.Street, entity.States!.Value, entity.ProfileId);
    }

    protected override IQuery<Result<User>> BuildGetByIdQuery(Guid id) => new GetUserByIdQuery(id);

    protected override IQuery<Result<ICollection<User>>> BuildGetAllQuery() => new GetUserAllQuery();

    protected override ICommand<Result<User>> BuildUpdateCommand(Guid id, User entity)
    {
        return new UpdateUserCommand(id, /* same fields as BuildCreateCommand */);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id) => new DeleteUserCommand(id);
}
```

(Use block bodies if that is what `WorkstationService` uses — match the existing style.)

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Users`)

```
Users/
├── Dtos/UserDto.cs                 // record (Guid Id, string FirstName, string LastName, string UserName, DateTime Birthdate,
│                                   //         string Email, AddressDto Address, States States, DateTime DateOfLastChange, Guid ProfileId)
├── Dtos/AddressDto.cs              // record (string Country, string State, string Province, string District, string Street)
├── Dtos/CreateUpdateUserDto.cs     // record (string FirstName, string LastName, string Password, DateTime Birthdate,
│                                   //         string Email, AddressDto Address, States States, Guid ProfileId)
├── Mappers/UserMap.cs              // ToDto(User), ToDto(IEnumerable<User>), ToEntity(CreateUpdateUserDto, DateTime now) → Result<User>
└── Endpoints/UserEndpoints.cs      // MapUserEndpoints(this IEndpointRouteBuilder app)
```

- `UserDto` **excludes `Password`**; nested `AddressDto` keeps the JSON shaped like the owned type.
- `States` is serialized as a number (`0` Inactive / `1` Active) because no `JsonStringEnumConverter` is configured
  in `AppConfigBuilderExtensions` (see Notes).
- `ProfileId` only — the `Profile` navigation is not loaded by `CrudRepository` and is not embedded.

Register in `WebApi/Extensions/EndpointsBuilderExtensions.cs` (after `MapWorkstationEndpoints()`):

```csharp
endpoints.MapUserEndpoints();
```

Endpoints — same shape as `WorkstationEndpoints`. `ToEntity` needs a timestamp for `DateOfLastChange`, so
`CreateUser` / `UpdateUser` also inject `IDateTimeProvider` (the handler overwrites it anyway):

```csharp
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

    // GetUserAll / GetUserById / UpdateUser / DeleteUser: same as WorkstationEndpoints
}
```

Mapper — return the `Result` so invalid input becomes a 400 instead of a 500:

```csharp
public static Result<User> ToEntity(this CreateUpdateUserDto createUpdateUserDto, DateTime now)
{
    return User.Create(new UserCreationData
    {
        FirstName = createUpdateUserDto.FirstName,
        LastName = createUpdateUserDto.LastName,
        Password = createUpdateUserDto.Password,
        Birthdate = createUpdateUserDto.Birthdate,
        Email = createUpdateUserDto.Email,
        Address = new Address(
            createUpdateUserDto.Address.Country,
            createUpdateUserDto.Address.State,
            createUpdateUserDto.Address.Province,
            createUpdateUserDto.Address.District,
            createUpdateUserDto.Address.Street),
        States = createUpdateUserDto.States,
        DateOfLastChange = now,
        ProfileId = createUpdateUserDto.ProfileId
    });
}
```

> Null `Address` in the JSON body would throw in the mapper → guard it (`createUpdateUserDto.Address is null` →
> `Result.Failure<User>(UserErrors.AddressIsIncomplete)`) or make the domain accept a nullable address.

### Step 4 — Verify

- `dotnet build` the solution.
- Run the WebApi (Development → migrations auto-applied) and exercise with OpenAPI / a `.http` file:
  create a profile → create user with its id → get by id → get all → update (incl. changing profile / state /
  password) → delete → get by id (404).
- Response bodies never contain `password`; `userName` is `F.LASTNAME`; `dateOfLastChange` changes on update.
- Negative cases → 400: empty `firstName` / `lastName`; password < 8 chars; invalid `email`; missing `birthdate`;
  missing / blank address part; `states: 5`; `profileId` = `00000000-…`; `profileId` not in `phonebook_profiles`
  (`ProfileErrors.ProfileNotFound`, not 500).
- `birthdate: "1990-05-10"` (no offset) → 201 and stored as `1990-05-10 00:00:00+00` (no Npgsql Kind exception).
- Check `dotnet ef migrations add` produces **no changes** (only setter visibility / ctor change). If a model diff
  appears, discard it and investigate before committing.

## Scope

**In scope**
- Domain: `UserCreationData` → primitives, `User` setter/ctor refactor, `Validate`, new `Update`,
  `CreateVersion7`, UTC birthdate normalization, extended/aligned `UserErrors`, `IUserService`.
- Application: remove legacy `Users/Commands|Queries`; 3 commands + 2 queries with handlers (profile existence check,
  `IDateTimeProvider`), `UserService`, DI registration.
- WebApi: `UserDto`, `AddressDto`, `CreateUpdateUserDto`, `UserMap`, `UserEndpoints`, endpoint registration.

**Out of scope**
- **Password hashing** (see Notes — strongly recommended as the next ticket).
- Authentication / authorization / login endpoint.
- Dedicated change-password endpoint, PATCH, or partial updates (PUT replaces all fields, including password).
- Embedding `Profile` or `Agendas` in the payload (only `ProfileId`).
- Unique indexes on `email` / `user_name`.
- Agenda CRUD endpoints (follow-up ticket).
- Pagination / filtering on `GET /users`.
- Unit / integration tests (no test projects exist yet under `test/`).

## Notes / Future Considerations

- **Passwords are stored in plain text** (`UserConfiguration` converts `Password` → `Value` directly). Exposing
  user CRUD makes this reachable from HTTP; hash with e.g. `PasswordHasher<User>` / BCrypt in a follow-up
  (`Password` VO would hold the hash, validation runs on the raw input before hashing). Do not log request bodies
  for `/users`.
- **Username collisions**: `F.LASTNAME` is not unique (John Doe / Jane Doe → `J.DOE`) and there is no unique index.
  Decide on a suffix strategy + unique index in a follow-up (needs a migration). Username is intentionally **not**
  regenerated on update, so renaming a user keeps their login name.
- **Unique email**: no unique index on `email`; consider one alongside the username fix.
- **Error mapping**: `UpdateUser` (following `WorkstationEndpoints`) maps every service failure to 404, so a PUT
  with invalid data that passes the WebApi mapper but fails in the handler (e.g. unknown `profileId`) returns 404.
  Same open item as MNPB-120 / MNPB-122 — map by error code later.
- **Delete with agendas**: `agendas.user_id_created` → `phonebook_users` with `NoAction`; deleting a user that created
  agendas throws `DbUpdateException` (→ 500). Consider soft-delete via `States.Inactive` instead of hard delete for
  users, or the shared `...InUse` → 409 fix.
- **Profile delete cascades** to users (`OnDelete(Cascade)` in the snapshot) — deleting a profile silently deletes its
  users. Worth revisiting (`Restrict`) in a follow-up.
- **Enum as string**: consider registering `JsonStringEnumConverter` in `ConfigureHttpJsonOptions` so `states`
  is `"Active"` / `"Inactive"` in JSON (global change, affects nothing else today).
- **Domain event**: `UserCreatedDomainEvent` has no handler yet; `Publisher` simply iterates zero handlers, so it is
  harmless. A welcome-email handler (`IEmailService` exists) is a natural follow-up.
- `GET /users` returns 404 on an empty table (same as other resources); 200 with `[]` is arguably more RESTful.
- Suggested commit split mirroring previous tickets: Domain+Application, then WebApi.
