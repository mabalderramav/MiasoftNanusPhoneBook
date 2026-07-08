namespace MiasoftNanus.PhoneBook.Domain.Common;

public abstract class Entity(Guid id)
{
    public Guid Id { get; init; } = id;
}