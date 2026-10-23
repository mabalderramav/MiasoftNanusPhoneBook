# MNPB-133 WebApi - Refactor: Partial User update (PATCH)

## Summary

Let clients change only the user fields they care about, without resending the whole user. This picks up the open
item in MNPB-130:

> **MNPB-130 — Out of scope:** PATCH / JSON Patch / any other partial update.

After this ticket:

| Verb   | Route                       | Body                    | Success         | Failure                                   |
|--------|-----------------------------|-------------------------|-----------------|-------------------------------------------|
| POST   | `/users`                    | `CreateUserDto`         | 201 `UserDto`   | 400 `Error`                               |
| PUT    | `/users/{id:guid}`          | `UpdateUserDto`         | 200 `UserDto`   | 404 `UserNotFound` / 400 other `Error`    |
| PATCH  | `/users/{id:guid}`          | `PatchUserDto` (**new**)| 200 `UserDto`   | 404 `UserNotFound` / 400 other `Error`    |
| PUT    | `/users/{id:guid}/password` | `ChangeUserPasswordDto` | 204             | 404 `UserNotFound` / 400 other `Error`    |

- `PATCH /users/{id}` follows **JSON Merge Patch** semantics (RFC 7396): every field is optional; a field that is
  absent keeps its current value. `address` is merged field by field (`{ "address": { "street": "..." } }` changes
  only the street).
- The merged result goes through the same domain validation as `PUT` (`User.Update` → `Validate`), so a PATCH can
  never leave a user in a state that a PUT would reject.
- The password is **not** patchable. `PUT /users/{id}/password` stays the only way to change it; a `password` field
  in a PATCH body is ignored, like in PUT (MNPB-130).
- `PUT`, `POST`, `GET`, `DELETE` and the password route keep their contract. No DB schema change. No new packages.

Example:

```http
PATCH /users/0192f3a4-...
Content-Type: application/merge-patch+json

{ "lastName": "Vaca", "address": { "street": "Av. Banzer 123" }, "states": 0 }
```

## Motivation

### Why PATCH at all

Since MNPB-130, `PUT /users/{id}` no longer needs the password, but it is still a full replace: to change one field
(e.g. deactivate a user via `states`, or fix a street) the client must `GET` the user and send back all eight fields
plus the full address. That is chatty, and it races: two clients editing different fields overwrite each other with
stale values. A partial update only sends (and only overwrites) what changed.

### Chosen option: JSON Merge Patch with a nullable DTO (not JSON Patch)

| Option                                      | Body                                               | Verdict |
|---------------------------------------------|----------------------------------------------------|---------|
| **JSON Merge Patch** (RFC 7396)             | the subset of fields to change                     | ✅ chosen |
| JSON Patch (RFC 6902)                        | `[{ "op": "replace", "path": "/lastName", "value": "..." }]` | ❌ |
| More dedicated routes (`/users/{id}/states`, ...) | one route per field                          | ❌ |

1. **Fits every `User` field.** All user details are required (`UserData` has only `required` non-nullable members),
   so "remove a field" never makes sense. That is the one thing JSON Patch (`remove`) and RFC 7396 (`null` = remove)
   add over a plain "absent = unchanged" DTO, and we do not need it. A record with nullable members expresses the
   whole contract.
2. **No new dependency.** JSON Patch on System.Text.Json needs `Microsoft.AspNetCore.JsonPatch.SystemTextJson`,
   a `JsonPatchDocument<T>` applied to a mutable target, and its own error handling for bad paths / ops. Merge Patch
   binds like any other minimal-API body.
3. **Same shape as the rest of the API.** Clients send the same JSON shape as `PUT`, just with fewer fields, and it
   shows up in OpenAPI as a normal schema.
4. **Dedicated routes do not scale.** The password got its own route because it has different rules (MNPB-130).
   Ordinary details do not; one route per field would multiply endpoints, commands and handlers.

### Where the merge happens

The merge needs the **current** user, which is only available after the handler loads it. So:

```
PatchUserDto (all optional)
  → UserMap.ToUserPatchData                          // DTO → domain patch, no validation
  → IUserService.PatchAsync(id, patch)
  → PatchUserCommand
  → PatchUserCommandHandler
      → userRepository.GetByIdAsync                  // 404 if missing
      → patch.HasChanges ? continue : return user    // empty patch = no-op (RFC 7396)
      → user.Patch(patch, now)                       // current ⊕ patch → UserData → User.Update (full validation)
      → profile check (only if profileId was sent)
      → SaveChanges
```

The merge lives in the domain (`User.Patch`) because only the entity knows its current values and its invariants.
It builds a full `UserData` from "patch value ?? current value" and calls the existing `Update`, so there is one
validation path and one apply path for both PUT and PATCH.

Constraints that shape the design:

1. **`null` cannot be told apart from "absent"** with a plain nullable DTO (System.Text.Json sets both to `null`).
   That is fine here: every field is required, so an explicit `null` is treated as "keep the current value".
   This is a **documented deviation** from RFC 7396 (where `null` means remove); see Notes.
2. **`DateOfLastChange` comes from the server.** `UserPatchData` does not carry it; the handler passes
   `dateTimeProvider.CurrentTime` to `User.Patch`, the same way `ChangePassword` takes it.
3. **Empty patch.** `{}` (or a body with only ignored / null fields) changes nothing. Saving would return 0 rows and
   end in `Error.CannotSaveChanges`, so the handler short-circuits and returns 200 with the current user, without
   bumping `DateOfLastChange`.
4. **Error mapping** reuses `UserEndpoints.ToErrorResult` from MNPB-130: `UserErrors.UserNotFound` → 404, anything
   else → 400.
5. **Content type.** Minimal APIs accept any `application/json` or `*+json` media type for a JSON body
   (`HttpRequest.HasJsonContentType`), so both `application/json` and `application/merge-patch+json` work without
   custom binding. Declare both in OpenAPI with `.Accepts<PatchUserDto>(...)`.

## Implementation

Work bottom-up. The build should be green after each step.

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Users`)

1. **`Entities/UserPatchData.cs`** (new) — the optional user details. Flat address fields, like the user commands:

```csharp
using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents a partial update of the user details. A <c>null</c> member keeps the current value.
/// It does not contain the password.
/// </summary>
public sealed class UserPatchData
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? Birthdate { get; init; }
    public string? Email { get; init; }
    public string? Country { get; init; }
    public string? State { get; init; }
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Street { get; init; }
    public States? States { get; init; }
    public Guid? ProfileId { get; init; }

    /// <summary>
    /// Gets whether at least one member is set.
    /// </summary>
    public bool HasChanges =>
        FirstName is not null || LastName is not null || Birthdate is not null || Email is not null
        || Country is not null || State is not null || Province is not null || District is not null
        || Street is not null || States is not null || ProfileId is not null;
}
```

2. **`Entities/User.cs`** — add `Patch` next to `Update`:

```csharp
/// <summary>
/// Applies a partial update: every member of <paramref name="data"/> that is set replaces the current value, the
/// others are kept. The merged details are validated like <see cref="Update"/>. The username and the password are
/// not changed.
/// </summary>
/// <param name="data">The details to change.</param>
/// <param name="dateOfLastChange">The date of the change.</param>
public Result Patch(UserPatchData data, DateTime dateOfLastChange)
{
    return Update(new UserData
    {
        FirstName = data.FirstName ?? FirstName!,
        LastName = data.LastName ?? LastName!,
        Birthdate = data.Birthdate ?? Birthdate,
        Email = data.Email ?? Email!.Value,
        Address = new Address(
            data.Country ?? Address!.Country,
            data.State ?? Address.State,
            data.Province ?? Address.Province,
            data.District ?? Address.District,
            data.Street ?? Address.Street),
        States = data.States ?? States!.Value,
        DateOfLastChange = dateOfLastChange,
        ProfileId = data.ProfileId ?? ProfileId
    });
}
```

   - `Update` already validates before applying, so a failed patch leaves the entity untouched (no half-applied
     state).
   - `Birthdate` from the DB is already UTC / date-only; `Apply` normalising it again is idempotent.
   - `UserName` is **not** regenerated when the first / last name changes — same behaviour as `PUT` today.

3. **`Services/IUserService.cs`** — add:

```csharp
/// <summary>
/// Asynchronously applies a partial update to the details of an existing user. The password is not changed.
/// </summary>
/// <param name="id">The unique identifier of the user to be updated.</param>
/// <param name="data">The details to change; unset members keep their current value.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A result containing the updated user, or an error if the operation failed.</returns>
Task<Result<User>> PatchAsync(Guid id, UserPatchData data, CancellationToken cancellationToken = default);
```

### Step 2 — Application (`MiasoftNanus.PhoneBook.Application/Users`)

1. **`UseCases/Commands/PatchUser/PatchUserCommand.cs`** (new):

```csharp
/// <summary>
/// Represents a command to partially update the details of a user. The password is changed by
/// <see cref="ChangeUserPassword.ChangeUserPasswordCommand"/>.
/// </summary>
public sealed record PatchUserCommand(Guid Id, UserPatchData Data) : ICommand<Result<User>>;
```

   Carrying `UserPatchData` instead of eleven nullable positional parameters keeps the record readable and avoids a
   second "nullable copy" of the update command. (If you prefer flat commands like `UpdateUserCommand`, flatten it
   and add a `UserCommandMapper.ToUserPatchData` — the handler logic does not change.)

2. **`UseCases/Commands/PatchUser/PatchUserCommandHandler.cs`** (new) — same shape as `UpdateUserCommandHandler`:

```csharp
/// <summary>
/// Applies and persists a partial update of the details of an existing user. The stored password hash is kept as is.
/// </summary>
public sealed class PatchUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<PatchUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(
        PatchUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure<User>(UserErrors.UserNotFound);

        // Nothing to change: no save (it would affect 0 rows) and no DateOfLastChange bump.
        if (!command.Data.HasChanges)
            return Result.Success(user);

        var patchResult = user.Patch(command.Data, dateTimeProvider.CurrentTime);
        if (patchResult.IsFailure)
            return Result.Failure<User>(patchResult.Error);

        if (command.Data.ProfileId is not null)
        {
            var profile = await profileRepository.GetByIdAsync(command.Data.ProfileId.Value, cancellationToken);
            if (profile is null)
                return Result.Failure<User>(ProfileErrors.ProfileNotFound);
        }

        userRepository.Update(user);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(user)
            : Result.Failure<User>(Error.CannotSaveChanges);
    }
}
```

   - The profile is only looked up when the patch sends `profileId`; an unchanged profile was valid when it was set.
   - Picked up by the assembly scan in `Application/DependencyInjection.AddApplication`; no manual registration.

3. **`Services/UserService.cs`** — add, next to `UpdateDetailsAsync`:

```csharp
public Task<Result<User>> PatchAsync(Guid id, UserPatchData data, CancellationToken cancellationToken = default)
{
    return Dispatcher.SendCommandAsync(new PatchUserCommand(id, data), cancellationToken);
}
```

4. **No change** to `UpdateUserCommand(Handler)`, `ChangeUserPassword*`, `CreateUser*`, `CrudCqrsService`.

### Step 3 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Users`)

1. **DTOs**
   - `Dtos/PatchAddressDto.cs` (new):

```csharp
/// <summary>
/// Data transfer object representing a partial user address. A missing member keeps the current value.
/// </summary>
public record PatchAddressDto(
    string? Country,
    string? State,
    string? Province,
    string? District,
    string? Street
);
```

   - `Dtos/PatchUserDto.cs` (new):

```csharp
/// <summary>
/// Data transfer object used for partially updating the details of a user (JSON Merge Patch). A missing or
/// <c>null</c> member keeps the current value. The password is changed through <see cref="ChangeUserPasswordDto"/>.
/// </summary>
public record PatchUserDto(
    string? FirstName,
    string? LastName,
    DateTime? Birthdate,
    string? Email,
    PatchAddressDto? Address,
    States? States,
    Guid? ProfileId
);
```

   Positional records bind missing JSON properties to `null` for nullable parameters, so every member is optional.
   No `ToString()` override: there is no secret in it.

2. **`Mappers/UserMap.cs`** — add:

```csharp
public static UserPatchData ToUserPatchData(this PatchUserDto patchUserDto)
{
    return new UserPatchData
    {
        FirstName = patchUserDto.FirstName,
        LastName = patchUserDto.LastName,
        Birthdate = patchUserDto.Birthdate,
        Email = patchUserDto.Email,
        Country = patchUserDto.Address?.Country,
        State = patchUserDto.Address?.State,
        Province = patchUserDto.Address?.Province,
        District = patchUserDto.Address?.District,
        Street = patchUserDto.Address?.Street,
        States = patchUserDto.States,
        ProfileId = patchUserDto.ProfileId
    };
}
```

   No `Result` and no early validation here: whether `""` is valid depends on the merged user, which only the
   domain sees. `"address": null` simply means "no address change".

3. **`Endpoints/UserEndpoints.cs`**

```csharp
userGroup.MapPut("/{id:guid}", UpdateUser);
userGroup.MapPatch("/{id:guid}", PatchUser)
    .Accepts<PatchUserDto>("application/merge-patch+json", "application/json");
userGroup.MapPut("/{id:guid}/password", ChangeUserPassword);
```

```csharp
private static async Task<IResult> PatchUser(
    Guid id,
    PatchUserDto patchUserDto,
    IUserService userService,
    CancellationToken cancellationToken)
{
    var result = await userService.PatchAsync(id, patchUserDto.ToUserPatchData(), cancellationToken);
    if (result.IsFailure)
        return ToErrorResult(result.Error);

    return TypedResults.Ok(result.Value.ToDto());
}
```

   - Check that `.Accepts<T>(...)` does not replace the default `application/json` request-body metadata in the
     generated OpenAPI document; if it does, keep both content types listed as above.
   - `IDateTimeProvider` is not needed in the endpoint: the handler stamps the date.

### Step 4 — Verify

- `dotnet build` the solution.
- `dotnet ef migrations add Check_MNPB133` must produce **no model changes**. Discard the migration afterwards.
- Run the WebApi (Development) and exercise it with OpenAPI / a `.http` file. Start from a user created with
  `POST /users` and note its `GET /users/{id}` response and `SELECT password, date_of_last_change FROM
  phonebook_users WHERE id = ...`.
  - `PATCH` `{ "lastName": "Vaca" }` → 200; only `lastName` and `dateOfLastChange` changed; `userName` unchanged;
    stored `password` byte-for-byte the same.
  - `PATCH` `{ "address": { "street": "Av. Banzer 123" } }` → 200; only `address.street` changed, the other four
    address fields kept.
  - `PATCH` `{ "states": 0 }` → 200; user is `Inactive`.
  - `PATCH` with `Content-Type: application/merge-patch+json` → same as with `application/json`.
  - `PATCH` `{}` → 200 with the current user; `date_of_last_change` **not** changed; no `UPDATE` in the EF log.
  - `PATCH` `{ "firstName": null }` → treated like `{}` → 200, nothing changed.
  - `PATCH` `{ "password": "Other1234!" }` → 200 no-op; stored hash unchanged.
  - Negative cases:
    - `PATCH` `{ "firstName": "" }` → 400 `UserErrors.FirstNameCannotBeNullOrWhitespace`; DB unchanged.
    - `PATCH` `{ "email": "nope" }` → 400 `UserErrors.InvalidEmail`.
    - `PATCH` `{ "address": { "street": " " } }` → 400 `UserErrors.AddressIsIncomplete`.
    - `PATCH` `{ "states": 5 }` → 400 `UserErrors.InvalidState`.
    - `PATCH` `{ "profileId": "00000000-0000-0000-0000-000000000000" }` → 400 `UserErrors.ProfileIdCannotBeEmpty`.
    - `PATCH` `{ "profileId": "<unknown guid>" }` → 400 `ProfileErrors.ProfileNotFound`.
    - `PATCH /users/{unknown-id}` with a valid body → 404 `UserErrors.UserNotFound`.
    - `PATCH` with `Content-Type: text/plain` → 415.
- Regression: `PUT /users/{id}`, `PUT /users/{id}/password`, `POST`, `GET`, `DELETE` behave as in MNPB-130.

## Scope

**In scope**
- Domain: `UserPatchData`, `User.Patch`, `IUserService.PatchAsync`.
- Application: `PatchUserCommand` + `PatchUserCommandHandler`, `UserService.PatchAsync`.
- WebApi: `PatchUserDto` / `PatchAddressDto`, `UserMap.ToUserPatchData`, `PATCH /users/{id:guid}` (accepting
  `application/json` and `application/merge-patch+json`), error mapping through `ToErrorResult`.

**Out of scope**
- JSON Patch (RFC 6902) / `JsonPatchDocument<T>`.
- Patching the password (stays on `PUT /users/{id}/password`) or the username.
- PATCH for the other resources (Profile, Location, WorkArea, Workstation, Agenda).
- Optimistic concurrency (`ETag` / `If-Match`, row version) for lost-update protection.
- Rejecting unknown properties (e.g. `password`) with 400.
- Regenerating the username when the first / last name changes.
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **`null` semantics**: RFC 7396 says `null` removes a member. Here it is ignored because no user field is
  removable. If an optional field is ever added to `User` (e.g. a nullable phone), "absent" and `null` will need to
  be told apart — e.g. an `Optional<T>` wrapper with a custom `JsonConverter`, or binding the body as `JsonObject`
  / `JsonElement` and checking `TryGetProperty`. Revisit the DTO then.
- **Other resources**: the pattern is reusable as is: `<Entity>PatchData` with nullable members + `HasChanges`,
  `<Entity>.Patch(data)` that merges into `<Entity>Data` and calls `Update`, a `Patch<Entity>Command(Id, Data)`
  handler. Agenda (many optional phone fields) is the one that would need the `null`-vs-absent handling above.
  A generic `ICrudService.PatchAsync` is not worth it until a second resource needs it.
- **Lost updates**: PATCH reduces collisions (only sent fields are written) but does not prevent them. An `ETag`
  derived from `DateOfLastChange` (or a Postgres `xmin` concurrency token) + `If-Match` → 412 is the natural
  follow-up, and would cover PUT as well.
- **Unknown properties**: as with PUT, a `password` in the body is silently dropped. `[JsonUnmappedMemberHandling
  (JsonUnmappedMemberHandling.Disallow)]` on `PatchUserDto` / `UpdateUserDto` would turn it into a 400; decide for
  both DTOs together.
- **Username**: neither PUT nor PATCH regenerates `UserName` when the name changes. If it should follow the name,
  do it once in `User.Update` so both routes get it.
- **Error mapping**: still the user-only `ToErrorResult`; `Error.CannotSaveChanges` should become a 500 in the shared
  `Error` → `IResult` helper noted since MNPB-120.
- Suggested commit split mirroring previous tickets: Domain+Application, then WebApi.
