# MNPB-137 WebApi - Refactor: Remove flat reference ids from AgendaDto

## Summary

Pick up the open item from MNPB-135:

> **MNPB-135 — Out of scope:** Removing the flat `workstationId` / `locationId` / `userIdCreated` from `AgendaDto`.

Since MNPB-135, every agenda response carries each reference **twice**: as a flat id and as an embedded object whose
`id` is the same value. This ticket removes the flat ids from the **response** DTO and makes the embedded objects
non-nullable. Request DTOs are not touched.

| DTO                     | Direction | `workstationId` / `locationId` / `userIdCreated` | `workstation` / `location` / `userCreated` |
|-------------------------|-----------|--------------------------------------------------|--------------------------------------------|
| `AgendaDto`             | response  | **removed**                                      | **non-nullable** (were `?`)                |
| `CreateUpdateAgendaDto` | request   | kept (unchanged)                                 | –                                          |
| `PatchAgendaDto`        | request   | `workstationId` / `locationId` kept (unchanged)  | –                                          |

Response before → after:

```jsonc
// before (MNPB-135)
{
  "id": "0192f3a4-...", "code": "A-001", "...": "...",
  "workstationId": "0192aaaa-...",
  "locationId": "0192bbbb-...",
  "userIdCreated": "0192cccc-...",
  "workstation": { "id": "0192aaaa-...", "code": "WS-01", "name": "Sistemas", "description": null, "workAreaId": "..." },
  "location": { "id": "0192bbbb-...", "code": "SCZ", "description": "Santa Cruz" },
  "userCreated": { "id": "0192cccc-...", "userName": "mbalderrama", "firstName": "Miguel", "lastName": "Balderrama" }
}

// after (MNPB-137)
{
  "id": "0192f3a4-...", "code": "A-001", "...": "...",
  "workstation": { "id": "0192aaaa-...", "code": "WS-01", "name": "Sistemas", "description": null, "workAreaId": "..." },
  "location": { "id": "0192bbbb-...", "code": "SCZ", "description": "Santa Cruz" },
  "userCreated": { "id": "0192cccc-...", "userName": "mbalderrama", "firstName": "Miguel", "lastName": "Balderrama" }
}
```

- **Breaking change** for response readers: `agenda.workstationId` → `agenda.workstation.id` (same for location and
  creator).
- Routes, status codes, request bodies, Domain, Application, Infrastructure: **unchanged**. No DB schema change.
  No new packages.

## Motivation

### Why remove them

1. **One source of truth per reference.** With both shapes, nothing says which one is authoritative. If the two
   ever disagree (e.g. a navigation fixed up from a stale tracked entity), clients cannot tell which is right.
   With only the object, they can't disagree.
2. **The ids were kept only for compatibility.** MNPB-135 added the objects as an additive change so existing
   readers kept working. That was a transition step, not a long-term contract. The longer both stay, the more
   clients start reading the flat ids and the harder removing them becomes.
3. **Smaller payloads and a clearer schema.** Three redundant GUIDs per row on `GET /agendas`, and an OpenAPI
   schema where `workstation` is nullable even though it is always present.

### Why the objects become non-nullable

MNPB-135 typed them `WorkstationDto?` / `LocationDto?` / `UserSummaryDto?` because a navigation *might* not be
loaded, and the flat id was the fallback. Without the flat ids, a `null` object would silently drop the reference
from the response. Every path that returns an `AgendaDto` already loads the three references (verified in MNPB-135):

| Endpoint | Source of the agenda                                    | References loaded by                     |
|----------|---------------------------------------------------------|------------------------------------------|
| GET      | `GetByIdWithDetailsAsync` / `GetAllWithDetailsAsync`    | `Include` × 3                            |
| POST     | `CreateAgendaCommandHandler`                            | `LoadDetailsAsync` after save            |
| PUT      | `UpdateAgendaCommandHandler`                            | `LoadDetailsAsync` after save            |
| PATCH    | `PatchAgendaCommandHandler` (incl. the `{}` no-op path) | `LoadDetailsAsync`                       |

So the response contract can state "always present". If a future path forgets to load them, it should **fail
loudly** (500 in development, caught by the verification below) rather than return an agenda with a missing
workstation.

### Why the request DTOs keep flat ids

Writing a reference means *pointing at* an existing row, so an id is the natural input; the embedded object is a
read model. Asking clients to send `{ "workstation": { "id": "..." } }` would invite them to send `code` / `name`
too, which would be ignored and look like a nested update. "Ids in, objects out" is the usual REST shape and keeps
POST / PUT / PATCH bodies unchanged.

### Options considered

| Option                                                          | Verdict |
|-----------------------------------------------------------------|---------|
| **Remove the flat ids now, objects non-nullable**               | ✅ chosen |
| Keep both (status quo)                                          | ❌ duplicated contract, see above |
| Mark the ids `[Obsolete]` / document as deprecated, remove later | ❌ no API versioning or deprecation channel exists yet; the API has no released external clients (assumption — confirm before merging) |
| Introduce API versioning (`/v2/agendas`)                        | ❌ disproportionate for one DTO; revisit when there are external consumers |

## Implementation

Single layer: `MiasoftNanus.PhoneBook.WebApi/Agendas`. The build should be green after the step.

### Step 1 — WebApi

1. **`Dtos/AgendaDto.cs`** — drop the three ids, make the objects non-nullable, update the summary:

```csharp
/// <summary>
/// Data transfer object representing an agenda, with its workstation, location and creator user embedded.
/// </summary>
public record AgendaDto(
    Guid Id,
    string Code,
    string FullName,
    string Email,
    bool Enabled,
    string? ImagePath,
    string? DirectLine,
    string? Internal,
    string? Corporate,
    string? DirectAccess,
    WorkstationDto Workstation,
    LocationDto Location,
    UserSummaryDto UserCreated
);
```

2. **`Mappers/AgendaMap.cs`** — `ToDto(Agenda)`:

```csharp
public static AgendaDto ToDto(this Agenda agenda)
{
    // The workstation, location and creator user are always loaded by the repository / command handlers.
    return new AgendaDto(
        agenda.Id,
        agenda.Code,
        agenda.FullName,
        agenda.Email!.Value,
        agenda.Enabled,
        agenda.ImagePath,
        agenda.DirectLine,
        agenda.Internal,
        agenda.Corporate,
        agenda.DirectAccess,
        agenda.Workstation!.ToDto(),
        agenda.Location!.ToDto(),
        agenda.UserCreated!.ToSummaryDto()
    );
}
```

   - `!` matches the existing style (`agenda.Email!.Value`, `user.UserName!.Value`). A missing navigation throws
     inside `ToDto` → 500, which is the "fail loudly" behaviour wanted above.
   - `ToAgendaPatchData` and `ToEntity` are **not** changed.

3. **No change** to `CreateUpdateAgendaDto`, `PatchAgendaDto`, `AgendaEndpoints`, `WorkstationDto`, `LocationDto`,
   `UserSummaryDto`, or any Domain / Application / Infrastructure type.

### Step 2 — Verify

- `dotnet build` the solution (only the existing `Microsoft.OpenApi` NU1903 warning expected).
- No EF model change is possible (WebApi only); no migration check needed.
- Run the WebApi:

```bash
dotnet run --project ./src/MiasoftNanus.PhoneBook.WebApi/MiasoftNanus.PhoneBook.WebApi.csproj \
  --launch-profile Development -- --ConnectionStrings:DefaultConnection='[Database Connection String]'
```

- **OpenAPI** (`GET https://localhost:5301/openapi/v1.json`):
  - `CreateUpdateAgendaDto` / `PatchAgendaDto` still have their id members.
  - There is **no** `AgendaDto` schema: the endpoints return `IResult` without `.Produces<T>()`, so no resource
    publishes a response schema (pre-existing). The response contract is checked on the JSON below instead.
- **Responses** — for each of `POST`, `GET /agendas/{id}`, `GET /agendas`, `PUT`, `PATCH { "enabled": false }`,
  `PATCH {}`:
  - no `workstationId` / `locationId` / `userIdCreated` keys in the JSON;
  - `workstation.id`, `location.id`, `userCreated.id` equal the ids sent on create / last update.
- **Requests unchanged**: `PUT` with a new `workstationId` → `workstation.id` is the new one; `PATCH`
  `{ "locationId": "..." }` → `location.id` is the new one; `PATCH { "userIdCreated": "..." }` still ignored.
- **Regression**: the MNPB-135 negative cases (400 / 404 / 415) and `DELETE` → 204 behave the same. The MNPB-135
  verification script can be reused, replacing `x["workstationId"]` with `x["workstation"]["id"]` (and likewise for
  location / creator) and adding an assertion that the three flat keys are absent.
- Delete the test agendas created during verification.

## Scope

**In scope**
- `AgendaDto`: remove `WorkstationId`, `LocationId`, `UserIdCreated`; `Workstation`, `Location`, `UserCreated`
  non-nullable.
- `AgendaMap.ToDto(Agenda)` accordingly.

**Out of scope**
- Request DTOs (`CreateUpdateAgendaDto`, `PatchAgendaDto`) — ids stay as input.
- Same treatment for other resources (`WorkstationDto.WorkAreaId`, `UserDto.ProfileId`, and `workAreaId` inside
  the embedded `workstation`).
- API versioning / deprecation headers.
- `?expand=` / selectable embedding.
- Taking `UserIdCreated` from the authenticated user (still from the body, MNPB-126 Notes).
- Unit / integration tests (no test projects yet).

## Notes / Future Considerations

- **Breaking change**: confirm no client (frontend, scripts, `.http` files outside this repo) reads the flat ids
  before merging, and call it out in the PR description.
- **Consistency across resources**: after this ticket `AgendaDto` is the only response with embedded references;
  `WorkstationDto` (`workAreaId`) and `UserDto` (`profileId`) still expose flat ids only. If the API should follow
  one convention, embed `workArea` / `profile` in a follow-up — the MNPB-135 loading pattern (`WithDetails` +
  `LoadDetailsAsync`) applies as is.
- **Embedded `workstation.workAreaId`**: the nested `WorkstationDto` still carries a flat id one level down. That is
  intended (one level of embedding only, MNPB-135) and goes away only if the previous point is done.
- **OpenAPI response schemas**: no endpoint declares `.Produces<T>()`, so the generated document has request
  schemas only. Adding `.Produces<AgendaDto>(200)` (and the same on the other resources) would publish the
  non-nullable `workstation` / `location` / `userCreated` contract to clients; good follow-up, API-wide.
- **Fail-loud mapping**: if a future command / query returns an `Agenda` without its references (e.g. a bulk
  operation), it must load them or use its own DTO; `AgendaMap.ToDto` will throw otherwise.
- Suggested commit: one commit (WebApi only).
