# MNPB-143 Application / WebApi - Use case: Delete photo of agenda, and delete it with the agenda

## Summary

Pick up one open item from MNPB-139 (also listed as out of scope in MNPB-141):

> **MNPB-139 — Out of scope:**
> - Delete-photo use case, and deleting the photo when the agenda is deleted.

Today a stored photo is only removed when it is **replaced** by a new upload (`UploadPhotoAgendaCommandHandler`
deletes the previous file after the save). There is no way to remove the photo of an agenda, and
`DeleteAgendaCommandHandler` removes the row but leaves the file in `Storage/agendas/` forever (orphan file).

After this ticket:

| Verb   | Route                      | Body | Success | Failure                                                        |
|--------|----------------------------|------|---------|----------------------------------------------------------------|
| DELETE | `/agendas/{id:guid}/photo` | –    | 204     | 404 `AgendaNotFound` / 404 `PhotoNotFound` / 400 other `Error` |
| DELETE | `/agendas/{id:guid}`       | –    | 204     | unchanged (404)                                                |

- **New use case** `DeletePhotoAgendaCommand`: sets `Agenda.ImagePath = null`, saves, then deletes the file.
- **`DeleteAgendaCommandHandler`** also deletes the photo file after the agenda row is removed.
- The three handlers that delete agenda photos (upload, delete photo, delete agenda) share one small helper with
  the `"agendas"` container and the "only files from this storage" rule, instead of copying it.
- No DB schema change (`image_path` is already nullable). No new packages. No change to `IFileStorage`.

```bash
curl -k -X DELETE https://localhost:5301/agendas/0192f3a4-.../photo   # 204, file removed
curl -k https://localhost:5301/agendas/0192f3a4-.../photo             # 404 PhotoNotFound
```

## Motivation

### Why a command (not `PATCH { "imagePath": null }`)

1. **PATCH cannot clear it.** `Agenda.Patch` uses `data.ImagePath ?? ImagePath`, so `null` means "keep". And even
   if it could, PATCH would only change the column: the file would stay on disk.
2. **Same shape as upload.** Removing the photo is a business operation on an agenda (it must exist, it must have a
   photo, DB and storage must change together). It mirrors `UploadPhotoAgendaCommand` and is the natural pair of
   `PUT /agendas/{id}/photo`.

### Why delete the file **after** the DB save

The database is the source of truth: a file not referenced by any agenda is harmless (only disk space), but an
agenda pointing to a missing file is a broken `GET /agendas/{id}/photo` (404 `PhotoNotFound`). So both handlers:

1. change / remove the agenda and `SaveChangesAsync`;
2. only if the save succeeded, delete the file (best effort, `CancellationToken.None`, like the upload handler does
   with the previous photo).

If the process crashes between 1 and 2, the result is an orphan file — the same accepted trade-off as MNPB-139
(Notes: "File + DB is not atomic").

### Why delete the photo inside `DeleteAgendaCommandHandler` (not a domain event)

| Option                                                                           | Verdict |
|----------------------------------------------------------------------------------|---------|
| **Delete the file in `DeleteAgendaCommandHandler`, after the save**              | ✅ chosen |
| `AgendaDeletedDomainEvent` + handler that deletes the file                       | ❌ domain events are dispatched by the unit of work; adds an event, a handler and a timing question (before / after commit) for a 2-line side effect |
| Endpoint `DELETE /agendas/{id}` calls `DeletePhotoAsync` then `DeleteAsync`       | ❌ two transactions, rule leaks into WebApi, other callers of `IAgendaService.DeleteAsync` would still leave orphans |
| Periodic clean-up job (delete files not referenced by any agenda)                 | ❌ for now; still useful later for crash orphans (see Notes) |

### Why a shared helper

`UploadPhotoAgendaCommandHandler` already has `Container = "agendas"` and the check
`previousImagePath.StartsWith($"{Container}/")` (never delete a path written through POST / PUT / PATCH that points
outside our storage). With two more handlers needing it, keep it in one place.

### `PhotoNotFound` when there is nothing to delete

`DELETE /agendas/{id}/photo` on an agenda without photo returns **404 `PhotoNotFound`**, the same error
`GET /agendas/{id}/photo` returns. It tells the client the state it assumed is wrong. (Making it an idempotent 204
is a one-line change in the handler if the team prefers it.)

## Implementation

Order: Domain → Application → WebApi. The build should be green after each step.

### Step 1 — Domain

1. **`Agendas/Entities/Agenda.cs`** — a focused method next to `ChangeImagePath`:

```csharp
/// <summary>
/// Removes the relative path of the agenda photo. Other members are not changed.
/// </summary>
public Result RemoveImagePath()
{
    if (ImagePath is null)
        return Result.Failure(AgendaErrors.PhotoNotFound);

    ImagePath = null;
    return Result.Success();
}
```

   - `PhotoNotFound` already exists ("The agenda does not have a photo."). No new errors.

2. **`Agendas/Services/IAgendaService.cs`** — add after `GetPhotoAsync`:

```csharp
/// <summary>
/// Asynchronously removes the photo of an existing agenda and deletes the stored file.
/// </summary>
/// <param name="id">The unique identifier of the agenda.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A result indicating success, or an error if the agenda or its photo does not exist.</returns>
Task<Result> DeletePhotoAsync(Guid id, CancellationToken cancellationToken = default);
```

### Step 2 — Application

1. **`Agendas/Storage/AgendaPhotoStorage.cs`** (new) — the container name and the safe delete shared by the
   handlers:

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

namespace MiasoftNanus.PhoneBook.Application.Agendas.Storage;

/// <summary>
/// Groups the storage rules of agenda photos.
/// </summary>
internal static class AgendaPhotoStorage
{
    /// <summary>
    /// The logical folder where agenda photos are stored.
    /// </summary>
    public const string Container = "agendas";

    /// <summary>
    /// Deletes a photo that is no longer referenced. Paths that do not come from this storage (e.g. set through
    /// POST / PUT / PATCH) are ignored.
    /// </summary>
    public static async Task DeleteUnreferencedPhotoAsync(this IFileStorage fileStorage, string? imagePath)
    {
        if (imagePath is null || !imagePath.StartsWith($"{Container}/", StringComparison.Ordinal))
            return;

        // The database is already committed: a failed delete only leaves an orphan file.
        await fileStorage.DeleteAsync(imagePath, CancellationToken.None);
    }
}
```

   - Whether to swallow `IOException` here (file locked by a concurrent `GET .../photo` on Windows, permissions)
     is a decision for this ticket: the DB change is already committed, so throwing turns a success into a 500.
     Recommended: `try { ... } catch (IOException) { }` plus `UnauthorizedAccessException`, with a comment. There
     is no `ILogger` in Application handlers today; do not add one only for this.

2. **`UploadPhotoAgendaCommandHandler.cs`** — use the helper (behavior unchanged):
   - `Container` → `AgendaPhotoStorage.Container`.
   - Replace the last `if (previousImagePath is not null && ...StartsWith...) await fileStorage.DeleteAsync(...)`
     with `await fileStorage.DeleteUnreferencedPhotoAsync(previousImagePath);`.
   - The compensation deletes of the **new** file (`storedFile.Path`) stay as they are: they are not
     "unreferenced previous photo" deletes.

3. **`Agendas/UseCases/Commands/DeletePhotoAgenda/DeletePhotoAgendaCommand.cs`** (new):

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeletePhotoAgenda;

/// <summary>
/// Represents a command to delete the photo of an agenda.
/// </summary>
public sealed record DeletePhotoAgendaCommand(Guid Id) : ICommand<Result>;
```

4. **`Agendas/UseCases/Commands/DeletePhotoAgenda/DeletePhotoAgendaCommandHandler.cs`** (new):

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Application.Agendas.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeletePhotoAgenda;

/// <summary>
/// Removes the photo of an existing agenda and deletes the stored file.
/// </summary>
public sealed class DeletePhotoAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage
) : ICommandHandler<DeletePhotoAgendaCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeletePhotoAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure(AgendaErrors.AgendaNotFound);

        var imagePath = agenda.ImagePath;
        var removeResult = agenda.RemoveImagePath();
        if (removeResult.IsFailure)
            return removeResult;

        agendaRepository.Update(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure(Error.CannotSaveChanges);

        // Only after the save: the agenda must never point to a deleted file.
        await fileStorage.DeleteUnreferencedPhotoAsync(imagePath);
        return Result.Success();
    }
}
```

   - Registered automatically by `DependencyInjection.AddApplication` (assembly scan).

5. **`DeleteAgendaCommandHandler.cs`** — inject `IFileStorage` and delete the photo after the save:

```csharp
public sealed class DeleteAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage
) : ICommandHandler<DeleteAgendaCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure(AgendaErrors.AgendaNotFound);

        var imagePath = agenda.ImagePath;
        agendaRepository.Remove(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure(Error.CannotSaveChanges);

        await fileStorage.DeleteUnreferencedPhotoAsync(imagePath);
        return Result.Success();
    }
}
```

   - Update the summary: "Deletes and persists removal of an existing agenda, and deletes its photo."

6. **`Agendas/Services/AgendaService.cs`** — implement the new member (same pattern as `GetPhotoAsync`):

```csharp
public Task<Result> DeletePhotoAsync(Guid id, CancellationToken cancellationToken = default)
{
    return Dispatcher.SendCommandAsync(new DeletePhotoAgendaCommand(id), cancellationToken);
}
```

### Step 3 — WebApi

1. **`Agendas/Endpoints/AgendaEndpoints.cs`** — map the route next to the other photo routes:

```csharp
agendaGroup.MapDelete($"/{{id:guid}}/photo", DeletePhotoAgenda);
```

```csharp
private static async Task<IResult> DeletePhotoAgenda(
    Guid id,
    IAgendaService agendaService,
    CancellationToken cancellationToken)
{
    var result = await agendaService.DeletePhotoAsync(id, cancellationToken);
    if (result.IsFailure)
        return ToErrorResult(result.Error);

    return TypedResults.NoContent();
}
```

2. **`ToErrorResult`** — `PhotoNotFound` is also a 404:

```csharp
// A missing agenda or photo is a 404; any other failure (validation, unknown workstation / location, ...) is a 400.
private static IResult ToErrorResult(Error error)
{
    return error == AgendaErrors.AgendaNotFound || error == AgendaErrors.PhotoNotFound
        ? TypedResults.NotFound(error)
        : TypedResults.BadRequest(error);
}
```

   - `Error` is a `record`, so `==` compares by value; `AgendaErrors.*` create a new instance on every call.
   - `DELETE /agendas/{id}` endpoint: no change.

### Step 4 — Verify

- `dotnet build` the solution (only the existing `Microsoft.OpenApi` NU1903 warning expected).
- No EF model change: `dotnet ef migrations add Check --project src/MiasoftNanus.PhoneBook.Infrastructure`
  should produce an empty migration (then remove it).
- Manual checks (`curl` / `.http` file):
  - upload a photo → `DELETE /agendas/{id}/photo` → 204; `imagePath` is `null` in `GET /agendas/{id}`; file gone
    from `Storage/agendas/`; `GET /agendas/{id}/photo` → 404 `PhotoNotFound`;
  - `DELETE /agendas/{id}/photo` again → 404 `PhotoNotFound`;
  - unknown id → 404 `AgendaNotFound`;
  - agenda with `imagePath` set through PUT to `../../x.png` → 204, `imagePath` cleared, **no file outside the
    storage touched**;
  - upload a photo → `DELETE /agendas/{id}` → 204, file gone;
  - `DELETE /agendas/{id}` on an agenda without photo → 204 (unchanged).
- Regression: upload twice still deletes the previous file; GET / POST / PUT / PATCH `/agendas` and
  `PUT` / `GET /agendas/{id}/photo` unchanged.

## Scope

**In scope**
- Domain: `Agenda.RemoveImagePath`, `IAgendaService.DeletePhotoAsync`.
- Application: `AgendaPhotoStorage` helper, `DeletePhotoAgendaCommand` + handler, `AgendaService.DeletePhotoAsync`,
  photo delete in `DeleteAgendaCommandHandler`, `UploadPhotoAgendaCommandHandler` uses the helper.
- WebApi: `DELETE /agendas/{id:guid}/photo`, `PhotoNotFound` → 404 in `ToErrorResult`.

**Out of scope**
- Removing `ImagePath` from `CreateUpdateAgendaDto` / `PatchAgendaDto` (MNPB-139 Notes).
- Replacing `imagePath` in `AgendaDto` with a photo URL / `hasPhoto` (MNPB-141 Notes).
- Clean-up job for orphan files (crash between save and delete, files from before this ticket).
- Soft delete of agendas (would keep the photo; not modeled today).
- Content sniffing, resize / thumbnails, blob storage adapter.
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **Existing orphans.** Agendas deleted before this ticket already left files in `Storage/agendas/`. They are not
  removed by this ticket; a one-off script or the future clean-up job can delete files not referenced by any
  `agenda.image_path`.
- **Paths outside the storage.** While `ImagePath` is writable through POST / PUT / PATCH, `RemoveImagePath` clears
  any value but the helper only deletes `agendas/...` paths, and `LocalFileStorage.ResolveInsideRoot` is a second
  guard. Once `ImagePath` is read-only outside the photo use cases, the prefix check could go away.
- **Concurrent GET.** `OpenReadAsync` opens files with `FileShare.Delete` (MNPB-141), so deleting a photo while it is
  being downloaded works on Linux and Windows; the download finishes with the old bytes.
- **Bulk deletes.** If agendas are ever deleted in bulk (e.g. `ExecuteDeleteAsync` or a cascade from `Location`),
  they skip this handler and leave files behind — another reason for the clean-up job.
- Suggested commits: one per step (Domain, Application, WebApi).
