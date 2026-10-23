# MNPB-130 WebApi - Refactor: Dedicated User change-password endpoint

## Summary

Stop forcing clients to resend (and the server to re-hash) the password on every user edit. Follow-up to
the open items in MNPB-124 and MNPB-128:

> **MNPB-124 — Out of scope:** Dedicated change-password endpoint, PATCH, or partial updates (PUT replaces all
> fields, including password).
>
> **MNPB-128 — Notes:** PUT re-hashes on every update: because PUT replaces every field, clients must resend the
> password on every edit. Making `password` optional on update (keep the hash when it is omitted) or adding a
> `PUT /users/{id}/password` endpoint is the natural next refactor.

After this ticket:

| Verb   | Route                       | Body                    | Success         | Failure                                   |
|--------|-----------------------------|-------------------------|-----------------|-------------------------------------------|
| POST   | `/users`                    | `CreateUserDto`         | 201 `UserDto`   | 400 `Error`                               |
| PUT    | `/users/{id:guid}`          | `UpdateUserDto` (**no password**) | 200 `UserDto` | 404 `UserNotFound` / 400 other `Error` |
| PUT    | `/users/{id:guid}/password` | `ChangeUserPasswordDto` | 204             | 404 `UserNotFound` / 400 other `Error`    |

- `PUT /users/{id}` updates the user's details and **never touches the password**. The stored hash is kept as is,
  so no hashing happens on a details update.
- `PUT /users/{id}/password` is the only way to change a password. It validates the raw value (non-empty, min 8),
  hashes it once and persists it.
- `POST /users`, `GET` and `DELETE` keep their contract. `UserDto` still has no password.
- No DB schema change.

## Motivation

### Chosen option: dedicated endpoint (not an optional password on PUT)

MNPB-128 lists two options. The dedicated endpoint is preferred:

1. **One responsibility per route.** A details edit and a credential change have different rules (a future login
   ticket will want "current password" / re-authentication on the password route only). Mixing them in one PUT
   with an optional field makes that harder later.
2. **PUT stays a real replace of the user's details.** With an optional password, PUT would become "replace
   everything except a field that may or may not be there", which is PATCH semantics on a PUT verb.
3. **It fits the current code better.** The WebApi update path builds a full `User` through `User.Create`
   (`UserMap.ToEntity`), which requires a valid password. An optional password would need a "nullable password"
   path through `UserCreationData`, `Password`, `User.Validate`, `UpdateUserCommand` and the handler. A separate
   route needs none of that.

PATCH / JSON Patch stays out of scope (see Notes).

### Current update flow (MNPB-128)

```
CreateUpdateUserDto (password required)
  → UserMap.ToEntity → User.Create(data)                 // validates the raw password (early 400)
  → UserService.BuildUpdateCommand(entity.Password!.Value)
  → UpdateUserCommandHandler
      → user.Update(data)                                // Password VO = plain again
      → user.HashPassword(passwordHasher)                // PBKDF2 on every edit ❌
  → UserConfiguration: hash persisted (new salt every time)
```

Constraints that shape the design:

1. **`ICrudService<User, Guid>.UpdateAsync(Guid, User)` takes a `User`.** The only public way to build one is
   `User.Create`, which requires a password. A password-less update therefore cannot go through the generic
   `CrudCqrsService.UpdateAsync` from the WebApi. `IUserService` gets its own update method that takes the raw
   data instead of an entity.
2. **`Password` must be hashed before it is persisted** (`UserConfiguration` throws on a plain password). If
   `User.Update` no longer assigns `Password`, the loaded value is the one from `Password.FromHash`
   (`IsHashed = true`), so the guard passes without calling the hasher.
3. **Error mapping.** `UserEndpoints.UpdateUser` maps every service failure to 404 (open item since
   MNPB-120). Once the WebApi no longer pre-validates details through `User.Create`, validation errors come from the
   handler, and a 404 for "first name is empty" would be wrong. Both user update routes therefore map by error:
   `UserErrors.UserNotFound` → 404, anything else → 400. (`Error` is a `record`, so `==` compares by value.)

## Implementation

Work bottom-up. The build should be green after each step.

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Users`)

1. **`Entities/UserData.cs`** (new) — the fields shared by create and update, i.e. today's `UserCreationData`
   without `Password`:

```csharp
namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents the user details used to create or update a User instance. It does not contain the password.
/// </summary>
public class UserData
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateTime Birthdate { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public required States States { get; init; }
    public required DateTime DateOfLastChange { get; init; }
    public required Guid ProfileId { get; init; }
}
```

2. **`Entities/UserCreationData.cs`** — inherit the shared fields and keep only the password:

```csharp
public sealed class UserCreationData : UserData
{
    public required string Password { get; init; }
}
```

   Callers that use object initializers (`UserMap.ToEntity`, `UserCommandMapper` for create) compile unchanged.

3. **`Entities/User.cs`**
   - `Create(UserCreationData data)`: validate the password, then the details, then apply both.
   - `Update(UserData data)`: validate and apply the details only. **Does not touch `Password`.**
   - New `ChangePassword`: validates the raw value, hashes it and stamps `DateOfLastChange`.
   - `HashPassword` stays (still used by the create handler).

```csharp
public static Result<User> Create(UserCreationData data)
{
    var passwordResult = Password.Create(data.Password);
    if (passwordResult.IsFailure)
        return Result.Failure<User>(passwordResult.Error);

    var validationResult = Validate(data);
    if (validationResult.IsFailure)
        return Result.Failure<User>(validationResult.Error);

    var usernameResult = UsernameServices.GenerateUsername(data.FirstName, data.LastName);
    if (usernameResult.IsFailure)
        return Result.Failure<User>(usernameResult.Error);

    var user = new User
    {
        Id = Guid.CreateVersion7(),
        UserName = usernameResult.Value,
        Password = passwordResult.Value
    };
    user.Apply(data);

    user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
    return Result.Success(user);
}

/// <summary>
/// Updates the details of this user after validating the provided data. The username and the password are not
/// changed; use <see cref="ChangePassword"/> for the password.
/// </summary>
public Result Update(UserData data)
{
    var validationResult = Validate(data);
    if (validationResult.IsFailure)
        return validationResult;

    Apply(data);
    return Result.Success();
}

/// <summary>
/// Validates the raw password, replaces the current one with its hash and records the change date.
/// </summary>
public Result ChangePassword(string password, IPasswordHasher passwordHasher, DateTime dateOfLastChange)
{
    var passwordResult = Password.Create(password);
    if (passwordResult.IsFailure)
        return Result.Failure(passwordResult.Error);

    Password = passwordResult.Value.Hash(passwordHasher);
    DateOfLastChange = dateOfLastChange;
    return Result.Success();
}

private void Apply(UserData data)
{
    FirstName = data.FirstName;
    LastName = data.LastName;
    // A birthdate is a calendar date: keep it without timezone shifting, as UTC for timestamp.
    Birthdate = DateTime.SpecifyKind(data.Birthdate.Date, DateTimeKind.Utc);
    Email = Email.Create(data.Email).Value;
    Address = data.Address;
    States = data.States;
    DateOfLastChange = data.DateOfLastChange;
    ProfileId = data.ProfileId;
}

private static Result Validate(UserData data)
{
    // Same checks as today, minus the password block.
}
```

   - `Password = passwordResult.Value` inside the object initializer is valid (`private set`, inside `User`).
     It also removes the second `Password.Create` call that `Apply` did before.
   - **Error order on create changes slightly**: a request with both an empty first name and a short password now
     reports the password error first. Nothing depends on the order; mention it in the PR. (Alternatively, keep
     the order by validating details first and the password second — pick whichever reads better.)
   - `Validate` keeps the `data.Address` checks exactly as today (the WebApi guards the null address before
     building the data).

4. **`Services/IUserService.cs`** — add the two user-specific operations next to the inherited CRUD:

```csharp
/// <summary>
/// Provides a contract for managing user entities.
/// </summary>
public interface IUserService : ICrudService<User, Guid>
{
    /// <summary>
    /// Updates the details of an existing user. The password is not changed.
    /// </summary>
    Task<Result<User>> UpdateDetailsAsync(Guid id, UserData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the password of an existing user.
    /// </summary>
    Task<Result> ChangePasswordAsync(Guid id, string password, CancellationToken cancellationToken = default);
}
```

   The inherited `UpdateAsync(Guid, User)` remains (required by `CrudCqrsService`) but is no longer used by the
   endpoints. It maps to the same password-less `UpdateUserCommand`, so it is still correct if someone calls it.

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application`)

1. **`Abstractions/Services/CrudCqrsService.cs`** — make the dispatcher reachable from derived services:

```csharp
protected IDispatcher Dispatcher { get; } = dispatcher;
```

   (`private` → `protected`. Using the captured primary-constructor parameter `dispatcher` in `UserService` would
   raise CS9124 because it is also passed to the base constructor.)

2. **`Users/UseCases/Commands/UpdateUser/UpdateUserCommand.cs`** — drop `Password`. The record no longer carries a
   secret, so remove the `ToString()` override and let the compiler-generated one print it.

3. **`Users/UseCases/Commands/UpdateUser/UpdateUserCommandHandler.cs`** — remove `IPasswordHasher` and the
   `user.HashPassword(passwordHasher)` call. The rest (not found → `UserErrors.UserNotFound`, `user.Update(...)`,
   profile check, save) stays.

4. **`Users/UseCases/Commands/ChangeUserPassword/`** (new folder, same shape as the other commands):

```csharp
/// <summary>
/// Represents a command to change the password of a user.
/// </summary>
public sealed record ChangeUserPasswordCommand(Guid Id, string Password) : ICommand<Result>
{
    // Masks the raw password so it never leaks through logs.
    public override string ToString() => $"{nameof(ChangeUserPasswordCommand)} {{ Id = {Id}, Password = *** }}";
}
```

```csharp
/// <summary>
/// Validates, hashes and persists a new password for an existing user.
/// </summary>
public sealed class ChangeUserPasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IPasswordHasher passwordHasher
) : ICommandHandler<ChangeUserPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(
        ChangeUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var changeResult = user.ChangePassword(command.Password, passwordHasher, dateTimeProvider.CurrentTime);
        if (changeResult.IsFailure)
            return changeResult;

        userRepository.Update(user);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success()
            : Result.Failure(Error.CannotSaveChanges);
    }
}
```

   The handler is picked up by the assembly scan in `Application/DependencyInjection.AddApplication`; no manual
   registration. Check that `DeleteUserCommandHandler` uses the same `Result` / `Result.Failure(...)` shapes and
   match it.

5. **`Users/Mappers/UserCommandMapper.cs`** — the update overload now returns `UserData` (no `Password`):

```csharp
public static UserData ToUserData(this UpdateUserCommand command, DateTime now)
{
    return new UserData
    {
        FirstName = command.FirstName,
        // ... same fields as today, without Password
        DateOfLastChange = now,
        ProfileId = command.ProfileId
    };
}
```

   Rename it to `ToUserData` (it no longer builds `UserCreationData`) and update the call in
   `UpdateUserCommandHandler`.

6. **`Users/Services/UserService.cs`**
   - `BuildUpdateCommand(Guid id, User entity)`: drop `entity.Password!.Value`.
   - Add the two `IUserService` methods:

```csharp
public Task<Result<User>> UpdateDetailsAsync(Guid id, UserData data, CancellationToken cancellationToken = default)
{
    var command = new UpdateUserCommand(
        id,
        data.FirstName,
        data.LastName,
        data.Birthdate,
        data.Email,
        data.Address.Country,
        data.Address.State,
        data.Address.Province,
        data.Address.District,
        data.Address.Street,
        data.States,
        data.ProfileId);

    return Dispatcher.SendCommandAsync(command, cancellationToken);
}

public Task<Result> ChangePasswordAsync(Guid id, string password, CancellationToken cancellationToken = default)
{
    return Dispatcher.SendCommandAsync(new ChangeUserPasswordCommand(id, password), cancellationToken);
}
```

   Use block or expression bodies to match the rest of `UserService` (it currently uses block bodies).

7. **Create path — no change**: `CreateUserCommand`, `CreateUserCommandHandler` (still calls `HashPassword`) and
   `BuildCreateCommand` stay as they are.

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Users`)

1. **DTOs** — split `CreateUpdateUserDto`:
   - `Dtos/CreateUserDto.cs` — today's `CreateUpdateUserDto` renamed (keeps `Password` and the redacted
     `ToString()`).
   - `Dtos/UpdateUserDto.cs` — same fields **without** `Password`; plain record, no `ToString()` override needed.
   - `Dtos/ChangeUserPasswordDto.cs` — new:

```csharp
/// <summary>
/// Data transfer object used for changing the password of a user.
/// </summary>
public record ChangeUserPasswordDto(string Password)
{
    // Masks the raw password so it never leaks through logs.
    public override string ToString() => $"{nameof(ChangeUserPasswordDto)} {{ Password = *** }}";
}
```

   JSON for `POST /users` is unchanged (only the C# type name changes). A client that still sends `password` in
   `PUT /users/{id}` gets 200 and the field is **silently ignored** (System.Text.Json skips unknown properties by
   default) — call this out in the PR / release notes.

2. **`Mappers/UserMap.cs`**
   - `ToEntity(this CreateUserDto dto, DateTime now)` — unchanged apart from the parameter type.
   - New `ToUserData(this UpdateUserDto dto, DateTime now)` → `Result<UserData>`, with the same
     `Address is null` → `UserErrors.AddressIsIncomplete` guard. Domain validation of the details happens in the
     handler (`User.Update`); the endpoint maps its errors to 400.

3. **`Endpoints/UserEndpoints.cs`**

```csharp
userGroup.MapPut("/{id:guid}", UpdateUser);
userGroup.MapPut("/{id:guid}/password", ChangeUserPassword);
```

```csharp
private static async Task<IResult> UpdateUser(
    Guid id,
    UpdateUserDto updateUserDto,
    IUserService userService,
    IDateTimeProvider dateTimeProvider,
    CancellationToken cancellationToken)
{
    var userDataResult = updateUserDto.ToUserData(dateTimeProvider.CurrentTime);
    if (userDataResult.IsFailure)
        return TypedResults.BadRequest(userDataResult.Error);

    var result = await userService.UpdateDetailsAsync(id, userDataResult.Value, cancellationToken);
    if (result.IsFailure)
        return result.Error == UserErrors.UserNotFound
            ? TypedResults.NotFound(result.Error)
            : TypedResults.BadRequest(result.Error);

    return TypedResults.Ok(result.Value.ToDto());
}

private static async Task<IResult> ChangeUserPassword(
    Guid id,
    ChangeUserPasswordDto changeUserPasswordDto,
    IUserService userService,
    CancellationToken cancellationToken)
{
    // Early 400 on the raw value, before any lookup or hashing.
    var passwordResult = Password.Create(changeUserPasswordDto.Password);
    if (passwordResult.IsFailure)
        return TypedResults.BadRequest(passwordResult.Error);

    var result = await userService.ChangePasswordAsync(id, changeUserPasswordDto.Password, cancellationToken);
    if (result.IsFailure)
        return result.Error == UserErrors.UserNotFound
            ? TypedResults.NotFound(result.Error)
            : TypedResults.BadRequest(result.Error);

    return TypedResults.NoContent();
}
```

   - `IResult` with mixed `TypedResults` in a ternary needs a cast (`(IResult)TypedResults.NotFound(...)`) or an
     `if`/`return` pair; use whichever compiles cleanly and matches the file.
   - `CreateUser` only changes its DTO type.
   - `Error.CannotSaveChanges` → 400 on both routes. Not ideal (it is a server-side failure), but consistent with
     `CreateUser` today; see Notes.

### Step 4 — Verify

- `dotnet build` the solution.
- `dotnet ef migrations add Check_MNPB130` must produce **no model changes**. Discard the migration afterwards.
- Run the WebApi (Development) and exercise it with OpenAPI / a `.http` file:
  - `POST /users` with password `Secret123!` → 201 (unchanged).
  - Note the stored hash: `SELECT password, date_of_last_change FROM phonebook_users WHERE id = ...`.
  - `PUT /users/{id}` with new details and **no** `password` → 200; the stored `password` is **byte-for-byte the
    same** as before; `date_of_last_change` changed; put a breakpoint in `PasswordHasher.Hash` → not hit.
  - `PUT /users/{id}` with an extra `"password": "Other1234!"` → 200, and the stored hash is still unchanged.
  - `PUT /users/{id}/password` with `{ "password": "NewSecret1!" }` → 204; the stored hash changed, still starts
    with `AQAAAA`; `date_of_last_change` changed; `GET /users/{id}` → 200.
  - Temporary sanity check (do not commit): `Verify("NewSecret1!", storedHash)` → `true`,
    `Verify("Secret123!", storedHash)` → `false`.
  - Negative cases:
    - `PUT /users/{id}/password` with `""` → 400 `Error.NullValue`; `"short"` → 400 `UserErrors.InvalidPassword`;
      no DB lookup (early return).
    - `PUT /users/{unknown-id}/password` with a valid password → 404 `UserErrors.UserNotFound`, and the hasher is
      **not** called (lookup happens before `ChangePassword`).
    - `PUT /users/{id}` with empty `firstName` → **400** `UserErrors.FirstNameCannotBeNullOrWhitespace`
      (was 404 before this ticket via the mapper path; now from the handler, mapped to 400).
    - `PUT /users/{id}` with unknown `profileId` → **400** `ProfileErrors.ProfileNotFound` (was 404).
    - `PUT /users/{unknown-id}` with valid data → 404 `UserErrors.UserNotFound`.
    - `PUT /users/{id}` with `"address": null` → 400 `UserErrors.AddressIsIncomplete`.
- `Console.WriteLine(command)` / `dto` for `ChangeUserPasswordCommand` / `ChangeUserPasswordDto` while debugging →
  prints `***`, never the password.

## Scope

**In scope**
- Domain: `UserData` (shared details), `UserCreationData : UserData`, `User.Create` / `Update(UserData)` /
  `ChangePassword`, `IUserService.UpdateDetailsAsync` / `ChangePasswordAsync`.
- Application: `CrudCqrsService.Dispatcher` → `protected`; password-less `UpdateUserCommand` + handler (no
  hashing); `ChangeUserPasswordCommand` + handler; `UserCommandMapper.ToUserData`; `UserService` new methods.
- WebApi: `CreateUserDto` / `UpdateUserDto` / `ChangeUserPasswordDto`, `UserMap.ToUserData`,
  `PUT /users/{id:guid}/password`, error mapping by `UserErrors.UserNotFound` on both user update routes.

**Out of scope**
- PATCH / JSON Patch / any other partial update.
- Requiring the **current** password (or re-authentication) to change it — needs login / auth first.
- Login / authentication endpoint, JWT, authorization, rehash-on-login.
- Password strength rules beyond the current min-8.
- Generic error-code → status mapping for the other resources (WorkArea, Workstation, Profile, Agenda).
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **Current-password check**: once login exists, `PUT /users/{id}/password` should require the current password
  (`IPasswordHasher.Verify`) or an authenticated caller who owns / administers the user. Keep it on this route only;
  the details route must not need it.
- **Domain event**: a `UserPasswordChangedDomainEvent` (e.g. to send a "your password was changed" email via
  `IEmailService`) is a natural follow-up; not added here because nothing would handle it yet.
- **Silent ignore of `password` on PUT**: clients built against MNPB-124/128 keep working, but a password they send
  in `PUT /users/{id}` is dropped. If that is a concern, reject it explicitly (e.g. `UnmappedMemberHandling =
  Disallow` on `UpdateUserDto` via `[JsonUnmappedMemberHandling]`) so the client gets a 400.
- **Error mapping**: this ticket fixes the 404-for-everything only on the user update routes. `CreateUser` and the
  other resources still use coarse mappings, and `Error.CannotSaveChanges` should become a 500 everywhere. A shared
  `Error` → `IResult` helper (by code / error type) is the follow-up already noted since MNPB-120.
- **Unused generic update**: `IUserService.UpdateAsync(Guid, User)` (inherited) stays for `ICrudService` symmetry.
  If it is still unused after the error-mapping refactor, consider whether `IUserService` should keep inheriting the
  full `ICrudService`.
- Suggested commit split mirroring previous tickets: Domain+Application, then WebApi.
