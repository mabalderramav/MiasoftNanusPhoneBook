# MNPB-139 Application - Use case: Upload photo of agenda

## Summary

Add the **upload photo** use case for an agenda: receive an image, store it in a file storage, and save its
relative path in `Agenda.ImagePath`.

The starting point is a sample `ArchivoService` (ASP.NET Core course). It writes an `IFormFile` to
`ContentRootPath/Almacenamiento/{subcarpeta}` with a random name and returns `ArchivoGuardado`
(`NombreArchivo`, `Ruta`, `Tamano`). This ticket keeps its good ideas and fits them into the project's layers:

| Sample (`ArchivoService`)                          | This project                                                                        |
|----------------------------------------------------|-------------------------------------------------------------------------------------|
| `ArchivoService` used directly by the endpoint     | `IFileStorage` port in **Application**, `LocalFileStorage` adapter in **Infrastructure** |
| `IFormFile` parameter (ASP.NET type)               | `Stream` + `fileName` + `length` (BCL types only, so Application has no ASP.NET dependency) |
| `IWebHostEnvironment.ContentRootPath`              | `FileStorageOptions.RootPath` from configuration (Infrastructure has no ASP.NET reference) |
| `ArchivoGuardado` class (serialized by the endpoint) | `StoredFile` record (internal result; the endpoint maps the `Agenda`, not the file) |
| `Path.GetRandomFileName()` (random `.xxx` extension) | `Guid.CreateVersion7():N` + **whitelisted** extension of the original file        |
| No size / type validation ("Clase 10")             | Size and extension validated in the handler, before anything is written             |
| Endpoint saves the file only                       | `UploadPhotoAgendaCommandHandler` saves the file **and** updates the agenda in one use case |

After this ticket:

- `IAgendaService.UploadPhotoAsync(id, content, fileName, length)` dispatches `UploadPhotoAgendaCommand`.
- The handler validates the photo, stores it under `agendas/`, sets `Agenda.ImagePath = "agendas/{name}.{ext}"`,
  saves, deletes the previous photo (if any) and returns the agenda with its details loaded (same as PATCH).
- No DB schema change (`image_path varchar(300)` already exists). No new packages.
- **No HTTP endpoint** in this ticket (Application use case only; see Scope).

## Motivation

### Why a port (`IFileStorage`) instead of the sample service as is

1. **Dependency rule.** Application references only Domain. `IFormFile` and `IWebHostEnvironment` live in
   ASP.NET Core; using them in a handler would pull the web framework into the Application layer. The project
   already solves this for other side effects with a port in Application and an adapter in Infrastructure:
   `IEmailService` → `EmailService`, `IDateTimeProvider` → `DateTimeProvider`.
2. **Swappable storage.** A local folder is fine for development. Later it can be replaced by blob storage
   (Azure / S3 / MinIO) by registering a different `IFileStorage`. The handler does not change.
3. **Testability.** A handler that takes a `Stream` and an `IFileStorage` can be tested with an in-memory fake.

### Why a command (not a method in the storage service)

Uploading a photo is a **business operation on an agenda**: the agenda must exist, the file must follow rules,
and `ImagePath` must change. That matches the other agenda use cases (`PatchAgendaCommand`,
`ChangeUserPasswordCommand`). The storage only knows how to write and delete bytes.

### Kept from the sample

- Never use the client's file name as the stored name (path traversal / overwrite).
- Build physical paths with `Path.Combine`; store a **relative** path with `/` in the DB.
- Create the target folder if it does not exist (`Directory.CreateDirectory`).
- Return a small result object with name, relative path and size.

### Changed from the sample

- **Extension is kept, but only from a whitelist** (`.jpg`, `.jpeg`, `.png`, `.webp`). `Path.GetRandomFileName()`
  gives names like `abc12def.x3k`, which a static file server cannot serve with the right content type.
- **Validation before writing**: empty file, max size, extension. The sample defers this.
- **Clean-up on failure**: if saving the agenda fails, the new file is deleted (no orphan files). After success,
  the old photo is deleted (best effort).
- **Root folder from configuration** instead of `IWebHostEnvironment`, because Infrastructure does not reference
  ASP.NET Core.

### Options considered

| Option                                                                 | Verdict |
|------------------------------------------------------------------------|---------|
| **`IFileStorage` port (Application) + `LocalFileStorage` (Infrastructure) + command** | ✅ chosen |
| Copy `ArchivoService` into WebApi and call it from the endpoint        | ❌ business rules (agenda exists, size, type, `ImagePath`) end up in the endpoint |
| Put `IFileStorage` in Domain (like `IPasswordHasher`)                  | ❌ storage is an application concern, not a domain rule; `IEmailService` / `IDateTimeProvider` are the closer precedent |
| Store the image in the DB (`bytea`)                                    | ❌ bigger rows and backups, every `GET /agendas` heavier; a path is already modeled |
| Use FluentValidation for the photo rules                               | ❌ the package is referenced but no validator exists yet; keep the `Result` + `Error` style used by every handler |

## Implementation

Order: Domain → Application → Infrastructure. The build should be green after each step.

### Step 1 — Domain

1. **`Agendas/Errors/AgendaErrors.cs`** — add photo errors (same style as the existing ones):

```csharp
public static Error PhotoCannotBeEmpty =>
    new Error("AgendaErrors.PhotoCannotBeEmpty", "The photo cannot be empty."
    );

public static Error PhotoTooLarge =>
    new Error("AgendaErrors.PhotoTooLarge", "The photo exceeds the maximum allowed size."
    );

public static Error PhotoInvalidFormat =>
    new Error("AgendaErrors.PhotoInvalidFormat", "The photo format is not allowed. Use jpg, jpeg, png or webp."
    );

public static Error ImagePathCannotBeNullOrWhitespace =>
    new Error("AgendaErrors.ImagePathCannotBeNullOrWhitespace", "Image path cannot be null or whitespace."
    );
```

2. **`Agendas/Entities/Agenda.cs`** — a focused method to change only the photo path (like
   `User.ChangePassword`), so the handler does not go through `Update` / `Patch`:

```csharp
/// <summary>
/// Changes the relative path of the agenda photo. Other members are not changed.
/// </summary>
/// <param name="imagePath">The relative path of the stored photo (e.g. <c>agendas/{name}.png</c>).</param>
public Result ChangeImagePath(string imagePath)
{
    if (string.IsNullOrWhiteSpace(imagePath))
        return Result.Failure(AgendaErrors.ImagePathCannotBeNullOrWhitespace);

    ImagePath = imagePath.Trim();
    return Result.Success();
}
```

3. **`Agendas/Services/IAgendaService.cs`** — add:

```csharp
/// <summary>
/// Asynchronously stores a new photo for an existing agenda and updates its image path.
/// </summary>
/// <param name="id">The unique identifier of the agenda.</param>
/// <param name="content">The photo content. The caller owns the stream.</param>
/// <param name="fileName">The original file name; only its extension is used.</param>
/// <param name="length">The photo size in bytes.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A result containing the updated agenda, or an error if the operation failed.</returns>
Task<Result<Agenda>> UploadPhotoAsync(
    Guid id,
    Stream content,
    string fileName,
    long length,
    CancellationToken cancellationToken = default);
```

### Step 2 — Application

1. **`Abstractions/Storage/StoredFile.cs`** — replaces `ArchivoGuardado`:

```csharp
namespace MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

/// <summary>
/// Represents a file saved by an <see cref="IFileStorage"/>.
/// </summary>
/// <param name="FileName">The generated file name (never the client's name).</param>
/// <param name="Path">The relative path with <c>/</c> separators (e.g. <c>agendas/{name}.png</c>); this is what
/// is stored in the database.</param>
/// <param name="Size">The size in bytes.</param>
public sealed record StoredFile(string FileName, string Path, long Size);
```

2. **`Abstractions/Storage/IFileStorage.cs`** — the port (replaces `ArchivoService`):

```csharp
namespace MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

/// <summary>
/// Defines the contract for saving and deleting files in a storage.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Asynchronously saves the content in the given container with a generated, unique name.
    /// </summary>
    /// <param name="content">The file content.</param>
    /// <param name="extension">The file extension, including the dot (e.g. <c>.png</c>). Already validated.</param>
    /// <param name="container">The logical folder (e.g. <c>agendas</c>).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The saved file.</returns>
    Task<StoredFile> SaveAsync(
        Stream content,
        string extension,
        string container,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes the file at the given relative path. Does nothing if it does not exist.
    /// </summary>
    /// <param name="path">The relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
```

3. **`Agendas/UseCases/Commands/UploadPhotoAgenda/UploadPhotoAgendaCommand.cs`** — replace the current empty
   record:

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UploadPhotoAgenda;

/// <summary>
/// Represents a command to upload the photo of an agenda.
/// </summary>
public sealed record UploadPhotoAgendaCommand(
    Guid Id,
    Stream Content,
    string FileName,
    long Length
) : ICommand<Result<Agenda>>;
```

4. **`Agendas/UseCases/Commands/UploadPhotoAgenda/UploadPhotoAgendaCommandHandler.cs`** — replace the current
   empty class:

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UploadPhotoAgenda;

/// <summary>
/// Validates and stores the photo of an existing agenda and persists its new image path.
/// </summary>
public sealed class UploadPhotoAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage
) : ICommandHandler<UploadPhotoAgendaCommand, Result<Agenda>>
{
    private const string Container = "agendas";
    private const long MaxPhotoSize = 2 * 1024 * 1024; // 2 MB

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    public async Task<Result<Agenda>> HandleAsync(
        UploadPhotoAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate before touching the storage or the database.
        if (command.Length <= 0)
            return Result.Failure<Agenda>(AgendaErrors.PhotoCannotBeEmpty);

        if (command.Length > MaxPhotoSize)
            return Result.Failure<Agenda>(AgendaErrors.PhotoTooLarge);

        // Only the extension of the client's file name is used, never the name itself.
        var extension = Path.GetExtension(command.FileName);
        if (!AllowedExtensions.Contains(extension))
            return Result.Failure<Agenda>(AgendaErrors.PhotoInvalidFormat);

        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<Agenda>(AgendaErrors.AgendaNotFound);

        var previousImagePath = agenda.ImagePath;
        var storedFile = await fileStorage.SaveAsync(
            command.Content, extension.ToLowerInvariant(), Container, cancellationToken);

        var changeResult = agenda.ChangeImagePath(storedFile.Path);
        if (changeResult.IsFailure)
        {
            await fileStorage.DeleteAsync(storedFile.Path, cancellationToken);
            return Result.Failure<Agenda>(changeResult.Error);
        }

        agendaRepository.Update(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
        {
            // The new file is not referenced by any agenda: remove it.
            await fileStorage.DeleteAsync(storedFile.Path, cancellationToken);
            return Result.Failure<Agenda>(Error.CannotSaveChanges);
        }

        // The previous photo is no longer referenced. Only files from this storage are removed.
        if (previousImagePath is not null && previousImagePath.StartsWith($"{Container}/", StringComparison.Ordinal))
            await fileStorage.DeleteAsync(previousImagePath, CancellationToken.None);

        await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);
        return Result.Success(agenda);
    }
}
```

   Notes:
   - `SaveChangesAsync` can also **throw** (DB down). Wrap the save in `try / catch` that deletes the new file
     and rethrows, so the compensation also runs on exceptions:

```csharp
int result;
try
{
    result = await unitOfWork.SaveChangesAsync(cancellationToken);
}
catch
{
    await fileStorage.DeleteAsync(storedFile.Path, CancellationToken.None);
    throw;
}
```

   - `MaxPhotoSize` and `AllowedExtensions` are constants in the handler on purpose (YAGNI). Move them to
     configuration only when another use case needs them.
   - The handler is registered automatically by `DependencyInjection.AddApplication` (assembly scan).

5. **`Agendas/Services/AgendaService.cs`** — implement the new member (same pattern as `PatchAsync`):

```csharp
public Task<Result<Agenda>> UploadPhotoAsync(
    Guid id,
    Stream content,
    string fileName,
    long length,
    CancellationToken cancellationToken = default)
{
    return Dispatcher.SendCommandAsync(
        new UploadPhotoAgendaCommand(id, content, fileName, length), cancellationToken);
}
```

### Step 3 — Infrastructure

1. **`Abstractions/Storage/FileStorageOptions.cs`**:

```csharp
namespace MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Storage;

/// <summary>
/// Settings of the local file storage.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// The root folder of the storage. A relative path is resolved against the current directory.
    /// </summary>
    public string RootPath { get; init; } = "Storage";
}
```

2. **`Abstractions/Storage/LocalFileStorage.cs`** — the adapter (the sample's logic, layered):

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

namespace MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Storage;

/// <summary>
/// Implements <see cref="IFileStorage"/> on the local file system.
/// </summary>
internal sealed class LocalFileStorage(FileStorageOptions options) : IFileStorage
{
    private readonly string _rootPath = Path.GetFullPath(options.RootPath);

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string extension,
        string container,
        CancellationToken cancellationToken = default)
    {
        // Path.Combine builds correct physical paths on any OS.
        var targetFolder = Path.Combine(_rootPath, container);
        Directory.CreateDirectory(targetFolder);

        // A generated name: the client's name could overwrite files or escape the folder.
        var fileName = $"{Guid.CreateVersion7():N}{extension}";
        var fullPath = Path.Combine(targetFolder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await content.CopyToAsync(stream, cancellationToken);
        }

        // The stored path uses "/" because it is a relative path for clients, not a physical path.
        return new StoredFile(fileName, $"{container}/{fileName}", new FileInfo(fullPath).Length);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, path));

        // Never delete outside the storage root (e.g. an ImagePath set to "../../x" through PUT / PATCH).
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            return Task.CompletedTask;

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
```

   - `FileMode.CreateNew` fails instead of overwriting if a name ever collided.
   - `Size` comes from the written file, not from the client's `length`.
   - If `CopyToAsync` throws (cancelled / client disconnect), delete the partial file and rethrow.

3. **`DependencyInjection.cs`** — register options and storage next to the other abstractions:

```csharp
var fileStorageOptions = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
                         ?? new FileStorageOptions();
services.AddSingleton(fileStorageOptions);
services.AddSingleton<IFileStorage, LocalFileStorage>();
```

   - `GetSection(...).Get<T>()` needs `Microsoft.Extensions.Configuration.Binder`. Check if it already comes
     transitively (EF Core / Npgsql). If not, read the value directly to avoid a new package:
     `new FileStorageOptions { RootPath = configuration["FileStorage:RootPath"] ?? "Storage" }`.

4. **WebApi `appsettings.json`** — document the setting (only config, no code):

```json
"FileStorage": {
  "RootPath": "Storage"
}
```

5. **`.gitignore`** — add `/src/MiasoftNanus.PhoneBook.WebApi/Storage/` so uploaded files are never committed.
   Do **not** use a bare `Storage/`: it would also ignore the `Abstractions/Storage/` code folders.

### Step 4 — Verify

- `dotnet build` the solution (only the existing `Microsoft.OpenApi` NU1903 warning expected).
- No EF model change: `dotnet ef migrations add Check --project src/MiasoftNanus.PhoneBook.Infrastructure`
  should produce an empty migration (then remove it).
- Without an endpoint, verify the use case with a temporary call (e.g. a scratch minimal-API route or a debug
  console) and delete it afterwards:
  - valid `.png` (≤ 2 MB) → `ImagePath = "agendas/{guid}.png"`, file exists under `Storage/agendas/`;
  - second upload → new path, old file deleted;
  - `length = 0` → `PhotoCannotBeEmpty`; 3 MB → `PhotoTooLarge`; `.gif` / `.exe` / no extension →
    `PhotoInvalidFormat`; unknown id → `AgendaNotFound`; in all of these **no file is written**;
  - file name `../../evil.png` → stored as `agendas/{guid}.png` (name ignored).
- Regression: `GET /agendas/{id}` returns the new `imagePath`; POST / PUT / PATCH / DELETE unchanged.

## Scope

**In scope**
- Domain: `AgendaErrors` photo errors, `Agenda.ChangeImagePath`, `IAgendaService.UploadPhotoAsync`.
- Application: `IFileStorage`, `StoredFile`, `UploadPhotoAgendaCommand` + handler, `AgendaService.UploadPhotoAsync`.
- Infrastructure: `FileStorageOptions`, `LocalFileStorage`, DI registration.
- Config: `FileStorage:RootPath` in `appsettings.json`, `Storage/` in `.gitignore`.

**Out of scope**
- WebApi endpoint `PUT /agendas/{id:guid}/photo` (`multipart/form-data`, `IFormFile` → `OpenReadStream()`,
  `DisableAntiforgery()`, request size limit) — follow-up ticket.
- Serving the photos (`UseStaticFiles` on the storage root, or a `GET /agendas/{id}/photo` endpoint).
- Delete-photo use case, and deleting the photo when the agenda is deleted.
- Content sniffing (magic bytes), image resize / thumbnails.
- Cloud blob storage adapter.
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **`ImagePath` is still writable through POST / PUT / PATCH.** A client can set any string, including a path
  outside the storage. `LocalFileStorage.DeleteAsync` guards against deleting outside the root, and the handler only
  deletes previous paths under `agendas/`. Removing `ImagePath` from `CreateUpdateAgendaDto` / `PatchAgendaDto`
  (photo only through the upload use case) is the natural next refactor.
- **Extension is not proof of format.** A `.png` name can carry any bytes. Checking the first bytes (PNG / JPEG /
  WebP signatures) in the handler would close this; it does not need a package.
- **File + DB is not atomic.** The handler compensates (delete the new file on failure, delete the old one after
  success). A crash between steps can still leave an orphan file; a periodic clean-up job can remove files not
  referenced by any agenda.
- **Root path**: `"Storage"` is relative to the current directory (the project folder with `dotnet run`, the
  publish folder in production). Set an absolute path per environment in `appsettings.{Env}.json`.
- **`UserIdCreated` / audit**: the upload does not record who changed the photo (same as PATCH today).
- Suggested commits: one per step (Domain, Application, Infrastructure + config).
