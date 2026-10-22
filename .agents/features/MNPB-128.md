# MNPB-128 WebApi - Refactor: Hash User passwords

## Summary

Stop storing `User.Password` in plain text. Follow-up to the first item in the MNPB-124
*Notes / Future Considerations*:

> Passwords are stored in plain text (`UserConfiguration` converts `Password` → `Value` directly). Exposing user CRUD
> makes this reachable from HTTP; hash with e.g. `PasswordHasher<User>` / BCrypt in a follow-up (`Password` VO would
> hold the hash, validation runs on the raw input before hashing). Do not log request bodies for `/users`.

After this ticket:

- The `password` column in `phonebook_users` only ever holds a hash (ASP.NET Core Identity `PasswordHasher<User>`,
  PBKDF2-HMAC-SHA512, self-describing v3 format).
- Password rules (non-empty, min 8 chars) are still validated on the **raw** input, before hashing.
- Hashing happens **once**, in the Create / Update command handlers.
- Plain text cannot reach the database. The EF converter refuses to persist an un-hashed `Password`.
- The raw password does not appear in `ToString()` output of the VO, the commands, or the DTO, so it cannot leak
  through logs. There is still no request-body logging for `/users`.

The HTTP contract stays the same: same routes, DTOs, status codes, and `UserDto` still has no password. The DB
schema also stays the same: `password` remains `text`.

## Motivation

Current password flow (MNPB-124):

```
CreateUpdateUserDto.Password (raw)
  → UserMap.ToEntity → User.Create(data)              // Password VO = raw
  → UserService.BuildCreateCommand(entity.Password!.Value)   // raw copied into the command
  → CreateUserCommandHandler → User.Create(data)      // Password VO = raw (built again)
  → UserConfiguration: password => password!.Value     // raw written to DB  ❌
```

Constraints that shape the design:

1. **`User.Create` runs twice**: once in `UserMap.ToEntity` (WebApi, early 400) and once in the handler.
   `CrudCqrsService` passes the entity's fields into the command. If `User.Create` hashed the password, the WebApi
   entity would carry a hash, the command would receive the hash, and the handler would validate and hash **the
   hash** (so the min-8 check would always pass and the stored hash would be of the wrong value). Hashing must
   therefore happen in the handler only, after the domain entity has validated the raw value.
2. **Domain cannot reference Application / Infrastructure**, so the hasher abstraction lives in the Domain
   (`Domain/Users/Services`, next to `IUserService` / `UsernameServices`) and its implementation lives in
   Infrastructure (same pattern as `IDateTimeProvider` → `DateTimeProvider`, except that one sits in Application).
3. **`Password` is a `record`**: the compiler-generated `ToString()` prints `Value`. The same applies to
   `CreateUserCommand`, `UpdateUserCommand` and `CreateUpdateUserDto`. Today nothing logs them (no `ILogger` in
   Application, no `UseSerilogRequestLogging` / `UseHttpLogging`, no `EnableSensitiveDataLogging`), but a single
   `logger.LogInformation("{Command}", command)` added later would leak the password.

## Implementation

Work bottom-up. The build should be green after each step.

### Step 1 — Domain (`MiasoftNanus.PhoneBook.Domain/Users`)

1. **`Services/IPasswordHasher.cs`** (new):

```csharp
namespace MiasoftNanus.PhoneBook.Domain.Users.Services;

/// <summary>
/// Hashes and verifies user passwords.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
```

   `Verify` is not used yet. It exists so the future login ticket does not have to change the abstraction.

2. **`ObjectValues/Password.cs`** — the VO now has two states. A *plain* password was validated but not hashed yet,
   and only lives in memory. A *hashed* password comes from `Hash` or from the DB.

```csharp
public sealed record Password
{
    private const int MinLength = 8;

    public string Value { get; }
    public bool IsHashed { get; }

    private Password(string value, bool isHashed)
    {
        Value = value;
        IsHashed = isHashed;
    }

    /// <summary>Validates a raw password. The result is NOT hashed yet.</summary>
    public static Result<Password> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Password>(Error.NullValue);

        return value.Length < MinLength
            ? Result.Failure<Password>(UserErrors.InvalidPassword)
            : Result.Success(new Password(value, isHashed: false));
    }

    /// <summary>Rehydrates a stored hash (EF Core).</summary>
    public static Password FromHash(string hash) => new(hash, isHashed: true);

    /// <summary>Returns the hashed form of this password. A password that is already hashed is returned as is.</summary>
    public Password Hash(IPasswordHasher passwordHasher) =>
        IsHashed ? this : new Password(passwordHasher.Hash(Value), isHashed: true);

    public override string ToString() => "***";
}
```

   - `sealed` + overriding `ToString()` on a record is allowed and stops `Value` from being printed.
     (Records also call `PrintMembers` from `ToString`. Overriding `ToString` is enough here.)
   - `Create` keeps its signature and its error codes, so `User.Validate` and the WebApi early-400 path behave as
     they do today.

3. **`Entities/User.cs`** — add one method. `Create`, `Update`, `Apply` and `Validate` stay the same: they still
   build the *plain* VO from the raw input.

```csharp
/// <summary>
/// Replaces the plain password with its hash. Must be called before the user is persisted.
/// </summary>
public void HashPassword(IPasswordHasher passwordHasher)
{
    Password = Password!.Hash(passwordHasher);
}
```

   The domain event and the username generation do not change.

### Step 2 — Infrastructure (`MiasoftNanus.PhoneBook.Infrastructure`)

1. **Package**: add `Microsoft.Extensions.Identity.Core` (10.0.x, matching the other `Microsoft.*` 10.0 packages) to
   `MiasoftNanus.PhoneBook.Infrastructure.csproj`. This package provides `PasswordHasher<TUser>` without pulling in
   the full Identity / EF stores.

   > BCrypt (`BCrypt.Net-Next`) is the alternative. `PasswordHasher<T>` is preferred because it is first-party, its
   > hash format is versioned, and `VerifyHashedPassword` returns `SuccessRehashNeeded`, which lets a future login
   > upgrade old hashes transparently.

2. **`Abstractions/Security/PasswordHasher.cs`** (new; same folder convention as `Abstractions/Time/DateTimeProvider.cs`):

```csharp
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Services;
using Microsoft.AspNetCore.Identity;

namespace MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Security;

/// <summary>
/// Implements <see cref="IPasswordHasher"/> using ASP.NET Core Identity's PBKDF2 <see cref="PasswordHasher{TUser}"/>.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    // PasswordHasher<TUser> does not use the user instance in its default implementation.
    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
```

   Fully qualify `Microsoft.AspNetCore.Identity.IPasswordHasher<T>` if the name clashes with the domain interface
   (it is generic, so a plain `using` should not be ambiguous).

3. **`DependencyInjection.cs`** — register it after `IEmailService`:

```csharp
services.AddSingleton<IPasswordHasher, PasswordHasher>();
```

   (The class is stateless, so a singleton is safe. Use `AddScoped` if you prefer to match the surrounding
   registrations.)

4. **`Users/UserConfiguration.cs`** — persist only hashes:

```csharp
builder.Property(u => u.Password)
    .HasConversion
    (
        password => password!.IsHashed
            ? password.Value
            : throw new InvalidOperationException("A plain-text password cannot be persisted."),
        value => Password.FromHash(value)
    ).IsRequired();
```

   `FromHash` replaces `Password.Create(value).Value`. Reading a stored hash through `Create` would re-run the raw
   password rules on it and mark it as plain.

   > EF Core does not allow a `throw` expression inside a converter lambda expression tree. If the compiler rejects
   > it, move the conversion to a private static method (`ToProvider(Password? password)`) and reference it from the
   > lambda. The guard can also live in `User.HashPassword` / the handlers, with the converter kept trivial. Keep
   > whichever version compiles cleanly. The guard is a safety net: the handlers should never hit it.

### Step 3 — Application (`MiasoftNanus.PhoneBook.Application/Users`)

1. **`CreateUserCommandHandler`** — inject `IPasswordHasher` and hash after validation and the profile check, just
   before `Add`:

```csharp
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IPasswordHasher passwordHasher
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

        userResult.Value.HashPassword(passwordHasher);

        userRepository.Add(userResult.Value);
        // ... unchanged
    }
}
```

   Hashing comes after the cheap checks because PBKDF2 is intentionally slow (~tens of ms), so a request with an
   unknown `profileId` should not pay for it.

2. **`UpdateUserCommandHandler`** — same pattern: `user.Update(...)` → profile check →
   `user.HashPassword(passwordHasher)` → `userRepository.Update(user)`. PUT always sends a password (MNPB-124 scope),
   so every update re-hashes it. A new salt is used each time, which is fine.

3. **Redact the commands** — `CreateUserCommand` and `UpdateUserCommand` are positional records that carry the raw
   `Password`. Override `PrintMembers` (or `ToString`) so the password is masked:

```csharp
public sealed record CreateUserCommand(/* ...unchanged... */) : ICommand<Result<User>>
{
    public override string ToString() =>
        $"{nameof(CreateUserCommand)} {{ FirstName = {FirstName}, LastName = {LastName}, Password = ***, ... }}";
}
```

   Choose the smallest form that reads well, for example `ToString() => $"{nameof(CreateUserCommand)} {{ ProfileId = {ProfileId} }}"`.
   The rule is simply that `Password` must not appear.

4. **`UserService` / `UserCommandMapper`** — no change. They keep moving the *plain* `entity.Password!.Value` into
   the command, which is correct because the WebApi-built entity is never hashed or persisted.

### Step 4 — WebApi (`MiasoftNanus.PhoneBook.WebApi/Users`)

1. **`Dtos/CreateUpdateUserDto.cs`** — apply the same `ToString()` redaction as for the commands.
2. **No request-body logging** — confirm and leave as is: `Program.cs` does not call `UseHttpLogging` /
   `UseSerilogRequestLogging`, and `AddDbContext` does not call `EnableSensitiveDataLogging`. Add a short comment near
   `app.MapEndpoints()` or in `AppLoggingBuilderExtensions` so nobody adds body logging without excluding `/users`:

```csharp
// Do not enable request/response body logging (UseHttpLogging with RequestBody) without excluding /users:
// it would log plain-text passwords.
```

3. `UserMap`, `UserDto` and `UserEndpoints` — no change. (`ToDto` already omits the password.)

### Step 5 — Existing data

`password` stays `text`, so **no schema migration** is needed. Rows that already exist hold plain text, though. After
this change they would be read as `Password.FromHash(plain)` and could never be verified by a future login.

- **Development**: delete the existing users (`DELETE FROM phonebook_users;`, or drop and re-migrate the dev DB) and
  re-create them through `POST /users`. Agendas that reference those users (`NoAction` FK) must be deleted first.
- **Staging / Production**: check first whether any user rows exist (`appsettings.Staging/Production.json` exist,
  so the API may have been deployed). If any do, add a one-off backfill **before** deploying (see Notes). Do not
  ship this ticket to an environment that has plain-text rows without the backfill.

### Step 6 — Verify

- `dotnet build` the solution.
- `dotnet ef migrations add Check_MNPB128` must produce **no model changes** (converter-only change). Discard the
  migration afterwards.
- Run the WebApi (Development) and exercise it with OpenAPI / a `.http` file:
  - `POST /users` with password `Secret123!` → 201. The response has no `password`.
  - `SELECT password FROM phonebook_users WHERE id = ...` → value starts with `AQAAAA` (Identity v3 format), is not
    `Secret123!`, and is ~84 chars long.
  - Create a second user with the **same** password → the stored hash is different (random salt).
  - `PUT /users/{id}` with a new password → the stored hash changes, and `GET /users/{id}` → 200 (the hash is read
    back through `FromHash` without errors).
  - Negative cases are unchanged: password `""` → 400 `Error.NullValue`; `"short"` → 400
    `UserErrors.InvalidPassword` (from the WebApi mapper, before any hashing).
  - A request with an unknown `profileId` → 400 `ProfileErrors.ProfileNotFound`, and no hashing happens (put a
    breakpoint in `PasswordHasher.Hash`).
- Temporary sanity check (do not commit): `new PasswordHasher().Verify("Secret123!", storedHash)` → `true`,
  `Verify("wrong", storedHash)` → `false`.
- Temporarily remove the `HashPassword` call from the create handler → `SaveChangesAsync` throws the converter's
  `InvalidOperationException` → 500. Restore the call afterwards. This proves that plain text cannot be persisted.
- `Console.WriteLine(command)` / `dto` / `user.Password` while debugging → prints `***`, never the password.

## Scope

**In scope**
- Domain: `IPasswordHasher`; `Password` VO with plain/hashed state, `FromHash`, `Hash`, and a redacted `ToString`;
  `User.HashPassword`.
- Infrastructure: `Microsoft.Extensions.Identity.Core` package, `PasswordHasher` implementation + DI registration,
  `UserConfiguration` converter (`FromHash` + plain-text guard).
- Application: hash in `CreateUserCommandHandler` / `UpdateUserCommandHandler`; redact `CreateUserCommand` /
  `UpdateUserCommand`.
- WebApi: redact `CreateUpdateUserDto`; "no body logging for /users" guard comment.
- Dev data reset instructions.

**Out of scope**
- Login / authentication endpoint, JWT, authorization (`IPasswordHasher.Verify` is ready for it).
- Rehash-on-login (`SuccessRehashNeeded`) — belongs to the login ticket.
- Dedicated change-password endpoint, or PATCH so that a PUT without a password keeps the existing hash.
- Password strength rules beyond the current min-8 (complexity, breached-password checks).
- Backfill tool for non-dev environments (only needed if such rows exist; see Step 5 / Notes).
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **Backfill** (only if a non-dev DB has users): Identity v3 hashes are base64 strings that decode to a leading
  `0x01` byte (prefix `AQAAAA`). Plain text cannot be hashed with SQL alone (PBKDF2), so the backfill would be a
  one-off .NET routine: load users, skip values that already look like v3 hashes, `Password.Create(value)` →
  `HashPassword` → save. Plain values shorter than 8 characters would fail `Create`; decide whether to force a
  reset for those users.
- **PUT re-hashes on every update**: because PUT replaces every field, clients must resend the password on every
  edit. Making `password` optional on update (keep the hash when it is omitted) or adding a
  `PUT /users/{id}/password` endpoint is the natural next refactor.
- **Hashing cost**: PBKDF2 with 100k iterations (Identity default) costs roughly 50–100 ms of CPU per create/update.
  This is acceptable for CRUD. Tune `PasswordHasherOptions.IterationCount` only with measurements.
- **Plain password lifetime**: the raw value still passes through `CreateUpdateUserDto` → WebApi `User` → command →
  handler `User` in memory. This is unavoidable because the hash has to be computed from it, but the raw value is
  never persisted, returned, or printed.
- **Error mapping**: `UpdateUser` still maps every service failure to 404 (open item from MNPB-120/122/124). This is
  unchanged here.
