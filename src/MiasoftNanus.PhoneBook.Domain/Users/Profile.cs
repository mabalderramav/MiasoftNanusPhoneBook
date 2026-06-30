using MiasoftNanus.PhoneBook.Domain.Common;

namespace MiasoftNanus.PhoneBook.Domain.Users;

public class Profile(string name, string description = "", Guid? id = null) : Entity(id ?? Guid.NewGuid())
{
    public string Name { get; } = name;
    public string Description { get; } = description;
}