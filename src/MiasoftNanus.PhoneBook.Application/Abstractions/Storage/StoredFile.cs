namespace MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

/// <summary>
/// Represents a file saved by an <see cref="IFileStorage"/>.
/// </summary>
/// <param name="FileName">The generated file name (never the client's name).</param>
/// <param name="Path">The relative path with <c>/</c> separators (e.g. <c>agendas/{name}.png</c>); this is what
/// is stored in the database.</param>
/// <param name="Size">The size in bytes.</param>
public sealed record StoredFile(string FileName, string Path, long Size);
