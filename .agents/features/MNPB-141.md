# MNPB-141 WebApi - Endpoints: Upload and serve the photo of an agenda

## Summary

Pick up two open items from MNPB-139:

> **MNPB-139 — Out of scope:**
> - WebApi endpoint `PUT /agendas/{id:guid}/photo` (`multipart/form-data`, `IFormFile` → `OpenReadStream()`,
>   `DisableAntiforgery()`, request size limit) — follow-up ticket.
> - Serving the photos (`UseStaticFiles` on the storage root, or a `GET /agendas/{id}/photo` endpoint).

MNPB-139 added the use case (`IAgendaService.UploadPhotoAsync` → `UploadPhotoAgendaCommandHandler`) and the storage
(`IFileStorage` / `LocalFileStorage`), but nothing on HTTP calls it, and a stored photo can not be downloaded.

After this ticket:

| Verb | Route                      | Body                                  | Success                     | Failure                                       |
|------|----------------------------|---------------------------------------|-----------------------------|-----------------------------------------------|
| PUT  | `/agendas/{id:guid}/photo` | `multipart/form-data`, field `photo`  | 200 `AgendaDto`             | 404 `AgendaNotFound` / 400 other `Error` / 413 |
| GET  | `/agendas/{id:guid}/photo` | –                                     | 200 image bytes (`image/*`) | 404 `AgendaNotFound` / 404 `PhotoNotFound`    |

- **PUT** binds an `IFormFile`, passes `OpenReadStream()` + `FileName` + `Length` to the existing use case and
  returns the agenda with its new `imagePath` (same response as `PATCH /agendas/{id}`).
- **GET** goes through a new query (`GetPhotoAgendaQuery`) and a new `IFileStorage.OpenReadAsync`, so the photo is
  served **through the storage port**, not from the disk directly (see Motivation).
- `AgendaDto.imagePath` is unchanged (relative path, e.g. `agendas/0192f3a4....png`). Clients show the photo with
  `GET /agendas/{id}/photo`.
- No DB schema change. No new packages (`IFormFile`, `FileExtensionContentTypeProvider` and
  `RequestSizeLimitAttribute` are in the ASP.NET Core shared framework).

Example:

```http
PUT /agendas/0192f3a4-.../photo
Content-Type: multipart/form-data; boundary=----x

------x
Content-Disposition: form-data; name="photo"; filename="juan.png"
Content-Type: image/png

<bytes>
------x--
```

```bash
curl -k -X PUT https://localhost:5301/agendas/0192f3a4-.../photo -F "photo=@juan.png"
curl -k https://localhost:5301/agendas/0192f3a4-.../photo -o juan.png
```

## Motivation

### Upload: `PUT /agendas/{id}/photo` with `multipart/form-data`

| Option                                                         | Verdict |
|----------------------------------------------------------------|---------|
| **`PUT /agendas/{id}/photo`, `multipart/form-data`, `IFormFile`** | ✅ chosen |
| `POST /agendas/{id}/photo`                                     | ❌ |
| Base64 string in a JSON body (or in `PATCH /agendas/{id}`)     | ❌ |
| Raw body (`Content-Type: image/png`, read `Request.Body`)      | ❌ |

- **PUT**: the photo is a sub-resource with a single value; uploading replaces it. Repeating the same request leaves
  the agenda in the same state (a new file name, but the same photo), which matches PUT semantics.
- **multipart**: standard for browsers (`<input type="file">`, `FormData`), Postman and `curl -F`; the original file
  name travels with the file, which the handler needs for the extension. Base64 is ~33% bigger and the JSON body
  has to be buffered whole; a raw body loses the file name.
- **`IFormFile` → `OpenReadStream()`**: keeps the Application layer free of ASP.NET types (MNPB-139 "Dependency
  rule"). The endpoint is the only place that knows about `IFormFile`.

### `DisableAntiforgery()`

Since .NET 8, a minimal API endpoint that binds `IFormFile` gets antiforgery metadata automatically. The pipeline
does not call `UseAntiforgery()` (and does not register antiforgery), so the request would fail with
`InvalidOperationException` ("Endpoint ... contains anti-forgery metadata, but a middleware was not found ...") →
500. Antiforgery protects **cookie-authenticated** browser forms against CSRF; this API has no cookie
authentication, so the check is disabled on this endpoint only.

> If cookie authentication is added later, revisit this: call `AddAntiforgery()` / `UseAntiforgery()` and remove
> `DisableAntiforgery()`, or make sure the auth is header-based (Bearer).

### Request size limit

Defaults are much larger than a photo: Kestrel `MaxRequestBodySize` = 30 MB, `FormOptions.MultipartBodyLengthLimit`
= 128 MB. Without a limit, a client can make the server read (and buffer to a temp file) 30 MB before the handler
answers `PhotoTooLarge`.

- The endpoint sets **3 MB** (`RequestSizeLimitAttribute` for Kestrel + `WithFormOptions` for the form reader).
- The **business rule stays in the handler** (2 MB, `PhotoTooLarge`). The HTTP limit is a little higher on purpose:
  multipart adds boundaries and headers, and a 2.5 MB photo should get the meaningful `PhotoTooLarge` 400, not a
  bare 413. Only clearly abusive bodies are cut at the transport level.

### Serving: endpoint through the storage port, not `UseStaticFiles`

| Option                                                                    | Verdict |
|---------------------------------------------------------------------------|---------|
| **`GET /agendas/{id}/photo` → query → `IFileStorage.OpenReadAsync`**       | ✅ chosen |
| `UseStaticFiles` with a `PhysicalFileProvider` on `FileStorage:RootPath`  | ❌ |
| `GET /agendas/{id}/photo` returning `TypedResults.PhysicalFile(...)`      | ❌ |

1. **Swappable storage (MNPB-139 Motivation, point 2).** `UseStaticFiles` and `PhysicalFile` read the local disk
   directly. Moving to blob storage would change the URLs clients use. Through `IFileStorage`, only the adapter
   changes.
2. **The URL is about the agenda, not the file.** `/agendas/{id}/photo` is stable when the photo is replaced;
   clients do not build URLs from `imagePath` or know the storage layout.
3. **Only referenced files are served.** Static files would serve any file under the root (old photos not yet
   deleted, orphans from a crash — MNPB-139 Notes). The query only opens the file the agenda points to.
4. **No middleware / root path in WebApi.** `UseStaticFiles` would need WebApi to read `FileStorage:RootPath`
   and resolve it the same way as `LocalFileStorage`; two places to keep in sync.

What static files give for free (ETag, `Last-Modified`, range requests, long caching) does not matter much for
small profile photos; see Notes.

### Why a query (not the endpoint calling `IFileStorage`)

`IFileStorage` lives in Application and WebApi only talks to Domain services (`IAgendaService`). Resolving the
photo needs the agenda (does it exist? which path?) and the storage — the same "business operation on an agenda"
reasoning as the upload command in MNPB-139.

### Content type

The stored name keeps a whitelisted extension (`.jpg`, `.jpeg`, `.png`, `.webp`, MNPB-139), so the content type is
derived from it in WebApi with `FileExtensionContentTypeProvider` (maps all four, including `image/webp`).
Application only returns the stream and the file name; HTTP content types are a WebApi concern.

## Implementation

Order: Domain → Application → Infrastructure → WebApi. The build should be green after each step.

### Step 1 — Domain

1. **`Agendas/Errors/AgendaErrors.cs`** — add (same style as the photo errors from MNPB-139):

```csharp
public static Error PhotoNotFound =>
    new Error("AgendaErrors.PhotoNotFound", "The agenda does not have a photo."
    );
```

2. **`Agendas/Entities/AgendaPhoto.cs`** — the result of the new query. It is in `Entities` next to
   `AgendaPatchData` / `AgendaCreationData` (the other non-entity types used by `IAgendaService`):

```csharp
namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents the stored photo of an agenda.
/// </summary>
/// <param name="Content">The photo content. The caller owns the stream and must dispose it.</param>
/// <param name="FileName">The stored file name (e.g. <c>{name}.png</c>); its extension gives the format.</param>
public sealed record AgendaPhoto(Stream Content, string FileName);
```

3. **`Agendas/Services/IAgendaService.cs`** — add:

```csharp
/// <summary>
/// Asynchronously opens the stored photo of an agenda.
/// </summary>
/// <param name="id">The unique identifier of the agenda.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A result containing the photo, or an error if the agenda or its photo does not exist.</returns>
Task<Result<AgendaPhoto>> GetPhotoAsync(Guid id, CancellationToken cancellationToken = default);
```

### Step 2 — Application

1. **`Abstractions/Storage/IFileStorage.cs`** — add a read method:

```csharp
/// <summary>
/// Asynchronously opens the file at the given relative path for reading.
/// </summary>
/// <param name="path">The relative path returned by <see cref="SaveAsync"/>.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A readable stream the caller must dispose, or <c>null</c> if the file does not exist.</returns>
Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default);
```

2. **`Agendas/UseCases/Queries/GetPhotoAgenda/GetPhotoAgendaQuery.cs`**:

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetPhotoAgenda;

/// <summary>
/// Represents a query to retrieve the photo of an agenda.
/// </summary>
public record GetPhotoAgendaQuery(Guid Id) : IQuery<Result<AgendaPhoto>>;
```

3. **`Agendas/UseCases/Queries/GetPhotoAgenda/GetPhotoAgendaQueryHandler.cs`**:

```csharp
using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetPhotoAgenda;

/// <summary>
/// Opens the stored photo of an agenda.
/// </summary>
public class GetPhotoAgendaQueryHandler(
    IAgendaRepository agendaRepository,
    IFileStorage fileStorage
) : IQueryHandler<GetPhotoAgendaQuery, Result<AgendaPhoto>>
{
    public async Task<Result<AgendaPhoto>> HandleAsync(
        GetPhotoAgendaQuery query,
        CancellationToken cancellationToken = default)
    {
        // Only the image path is needed: no details (workstation, location, user) are loaded.
        var agenda = await agendaRepository.GetByIdAsync(query.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<AgendaPhoto>(AgendaErrors.AgendaNotFound);

        if (string.IsNullOrWhiteSpace(agenda.ImagePath))
            return Result.Failure<AgendaPhoto>(AgendaErrors.PhotoNotFound);

        // A path set through POST / PUT / PATCH may point to a missing file (or outside the storage).
        var content = await fileStorage.OpenReadAsync(agenda.ImagePath, cancellationToken);
        if (content is null)
            return Result.Failure<AgendaPhoto>(AgendaErrors.PhotoNotFound);

        return Result.Success(new AgendaPhoto(content, Path.GetFileName(agenda.ImagePath)));
    }
}
```

   - `GetByIdAsync` is tracked (`CrudRepository`), but nothing is saved in this scope; it avoids the three
     `JOIN`s of `GetByIdWithDetailsAsync`, which matters when a list page asks one photo per row.
   - Registered automatically by `DependencyInjection.AddApplication` (assembly scan).

4. **`Agendas/Services/AgendaService.cs`** — implement the new member (same pattern as `UploadPhotoAsync`):

```csharp
public Task<Result<AgendaPhoto>> GetPhotoAsync(Guid id, CancellationToken cancellationToken = default)
{
    return Dispatcher.SendQueryAsync(new GetPhotoAgendaQuery(id), cancellationToken);
}
```

### Step 3 — Infrastructure

1. **`Abstractions/Storage/LocalFileStorage.cs`** — implement `OpenReadAsync` and extract the root guard already
   used by `DeleteAsync`, so both methods share it:

```csharp
public Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default)
{
    var fullPath = ResolveInsideRoot(path);
    if (fullPath is null || !File.Exists(fullPath))
        return Task.FromResult<Stream?>(null);

    // FileShare.Delete: a concurrent upload may delete this (previous) photo while it is being sent.
    Stream stream = new FileStream(
        fullPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
        bufferSize: 4096, useAsync: true);
    return Task.FromResult<Stream?>(stream);
}

public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
{
    var fullPath = ResolveInsideRoot(path);
    if (fullPath is not null && File.Exists(fullPath))
        File.Delete(fullPath);

    return Task.CompletedTask;
}

// Returns the physical path, or null if it would be outside the storage root
// (e.g., an ImagePath set to "../../x" through PUT / PATCH).
private string? ResolveInsideRoot(string path)
{
    var fullPath = Path.GetFullPath(Path.Combine(_rootPath, path));
    return fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        ? fullPath
        : null;
}
```

   - `File.Exists` + `new FileStream` can still race with a delete; catching `FileNotFoundException` and returning
     `null` makes it fully safe. Keep it if it stays short.

### Step 4 — WebApi

1. **`Agendas/Endpoints/AgendaEndpoints.cs`** — map the two routes in `MapAgendaEndpoints`:

```csharp
// Multipart overhead on top of the 2 MB photo rule of UploadPhotoAgendaCommandHandler.
private const long MaxPhotoRequestSize = 3 * 1024 * 1024;

private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
```

```csharp
agendaGroup.MapPut("/{id:guid}/photo", UploadPhotoAgenda)
    // IFormFile adds antiforgery metadata; this API has no cookie auth and no UseAntiforgery().
    .DisableAntiforgery()
    .WithMetadata(new RequestSizeLimitAttribute(MaxPhotoRequestSize))
    .WithFormOptions(multipartBodyLengthLimit: MaxPhotoRequestSize);
agendaGroup.MapGet("/{id:guid}/photo", GetPhotoAgenda);
```

2. Handlers (same style as the others; `ToErrorResult` already maps `AgendaNotFound` → 404, other → 400):

```csharp
private static async Task<IResult> UploadPhotoAgenda(
    Guid id,
    IFormFile photo,
    IAgendaService agendaService,
    CancellationToken cancellationToken)
{
    await using var content = photo.OpenReadStream();
    var result = await agendaService.UploadPhotoAsync(
        id, content, photo.FileName, photo.Length, cancellationToken);
    if (result.IsFailure)
        return ToErrorResult(result.Error);

    return TypedResults.Ok(result.Value.ToDto());
}

private static async Task<IResult> GetPhotoAgenda(
    Guid id,
    IAgendaService agendaService,
    CancellationToken cancellationToken)
{
    var result = await agendaService.GetPhotoAsync(id, cancellationToken);
    if (result.IsFailure)
        return TypedResults.NotFound(result.Error);

    if (!ContentTypeProvider.TryGetContentType(result.Value.FileName, out var contentType))
        contentType = "application/octet-stream";

    // TypedResults.Stream disposes the stream after the response is written.
    return TypedResults.Stream(result.Value.Content, contentType);
}
```

   - `IFormFile photo` → the form field must be named `photo`. A missing field or a non-form body is rejected by
     minimal API binding (400 / 415) before the handler runs.
   - The parameter is bound from the form automatically (`IFormFile`); `[FromForm]` is not needed. The OpenAPI
     document shows a `multipart/form-data` body with a binary `photo` field.
   - `GetPhotoAgenda` returns `NotFound` for both errors (`AgendaNotFound`, `PhotoNotFound`), like `GetAgendaById`.
   - Usings: `Microsoft.AspNetCore.Mvc` (`RequestSizeLimitAttribute`), `Microsoft.AspNetCore.StaticFiles`
     (`FileExtensionContentTypeProvider`).

### Step 5 — Verify

- `dotnet build` the solution (only the existing `Microsoft.OpenApi` NU1903 warning expected).
- No EF model change (`dotnet ef migrations add Check` → empty, then remove it).
- `/openapi/v1.json` shows `PUT /agendas/{id}/photo` with a `multipart/form-data` body and `GET /agendas/{id}/photo`.
- Upload (`curl -k -X PUT .../photo -F "photo=@file"`):
  - valid `.png` ≤ 2 MB → 200 `AgendaDto`, `imagePath = "agendas/{guid}.png"`, file exists under `Storage/agendas/`;
  - second upload → new `imagePath`, old file deleted;
  - `.gif` / `.exe` → 400 `PhotoInvalidFormat`; empty file → 400 `PhotoCannotBeEmpty`;
  - 2.5 MB → 400 `PhotoTooLarge`; 10 MB → 413 (rejected before the handler);
  - unknown id → 404 `AgendaNotFound`; no file written;
  - field named other than `photo`, or JSON body → 400 / 415, **not** 500 (antiforgery is disabled);
  - `-F "photo=@x.png;filename=../../evil.png"` → stored as `agendas/{guid}.png`.
- Serve (`curl -k -i .../photo`):
  - agenda with photo → 200, `Content-Type: image/png`, bytes identical to the upload (`cmp`);
  - agenda without photo → 404 `PhotoNotFound`; unknown id → 404 `AgendaNotFound`;
  - `imagePath` set to `"../../appsettings.json"` via `PATCH` → 404 `PhotoNotFound` (nothing outside the root
    is served); file deleted by hand → 404 `PhotoNotFound`.
- Regression: GET / POST / PUT / PATCH / DELETE `/agendas` unchanged.

## Scope

**In scope**
- Domain: `AgendaErrors.PhotoNotFound`, `AgendaPhoto`, `IAgendaService.GetPhotoAsync`.
- Application: `IFileStorage.OpenReadAsync`, `GetPhotoAgendaQuery` + handler, `AgendaService.GetPhotoAsync`.
- Infrastructure: `LocalFileStorage.OpenReadAsync` (+ shared root guard).
- WebApi: `PUT /agendas/{id:guid}/photo` (multipart, antiforgery disabled, 3 MB limit),
  `GET /agendas/{id:guid}/photo`.

**Out of scope**
- `DELETE /agendas/{id}/photo` (delete-photo use case) and deleting the photo when the agenda is deleted.
- Removing `ImagePath` from `CreateUpdateAgendaDto` / `PatchAgendaDto` (MNPB-139 Notes).
- Replacing `imagePath` in `AgendaDto` with a photo URL.
- HTTP caching (ETag / `Last-Modified` / `Cache-Control`) and range requests on the photo.
- Content sniffing (magic bytes), resize / thumbnails, blob storage adapter.
- Authentication / authorization on the photo endpoints (none of the endpoints have it yet).
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **Two size limits.** 2 MB (business rule, handler) and 3 MB (transport, endpoint) are separate constants. If the
  rule changes, change both; if it moves to configuration (MNPB-139 Notes), derive the HTTP limit from it.
- **`imagePath` in `AgendaDto`.** It is now an internal storage detail exposed to clients. A natural follow-up is
  `hasPhoto: bool` or `photoUrl: "/agendas/{id}/photo"` instead, together with making `ImagePath` read-only through
  POST / PUT / PATCH.
- **Caching.** The URL is stable while the photo changes, so it can not be cached forever. Cheap options later:
  an ETag from the stored file name (it changes on every upload, Guid v7) with `If-None-Match` → 304, or a
  versioned URL `?v={fileName}` with `Cache-Control: immutable`.
- **`X-Content-Type-Options: nosniff`** on the GET response avoids browsers sniffing a non-image payload with an
  image extension (extension is not proof of format, MNPB-139 Notes). One header; add it if a global security
  headers middleware does not arrive first.
- **Antiforgery** must be revisited if cookie authentication is added (see Motivation).
- Suggested commits: Domain + Application + Infrastructure (read side), then WebApi (both endpoints).
