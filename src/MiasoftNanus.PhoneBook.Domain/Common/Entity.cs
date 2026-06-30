using MiasoftNanus.PhoneBook.Domain.Common.Interfaces;

namespace MiasoftNanus.PhoneBook.Domain.Common;

public abstract class Entity(Guid id)
{
    public Guid Id { get; init; } = id;
    private readonly List<IDomainEvent> _domainEvents = [];

    public List<IDomainEvent> PopDomainEvents()
    {
        var tmp = _domainEvents.ToList();
        _domainEvents.Clear();
        return tmp;
    }
}