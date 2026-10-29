namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents the stored photo of an agenda.
/// </summary>
/// <param name="Content">The photo content. The caller owns the stream and must dispose it.</param>
/// <param name="FileName">The stored file name (e.g. <c>{name}.png</c>); its extension gives the format.</param>
public sealed record AgendaPhoto(Stream Content, string FileName);
