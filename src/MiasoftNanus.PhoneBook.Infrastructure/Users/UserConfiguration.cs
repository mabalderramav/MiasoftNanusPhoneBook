using MiasoftNanus.PhoneBook.Domain.Shared;
using MiasoftNanus.PhoneBook.Domain.Shared.Enums;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Email = MiasoftNanus.PhoneBook.Domain.Shared.ObjectValues.Email;

namespace MiasoftNanus.PhoneBook.Infrastructure.Users;

/// <summary>
/// Configures the database schema and relationships for the <see cref="User"/> entity.
/// </summary>
/// <remarks>
/// The <c>UserConfiguration</c> class is used to define the Entity Framework Core configuration
/// for the <see cref="User"/> entity, which includes setting up entity relationships, primary
/// keys, property configurations, and other database-level rules.
/// </remarks>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    void IEntityTypeConfiguration<User>.Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("phonebook_users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Password)
            .HasConversion
            (
                password => password!.Value,
                value => Password.Create(value).Value
            ).IsRequired();

        builder.Property(u => u.UserName)
            .HasConversion
            (
                nombreUsuario => nombreUsuario!.Value,
                value => Username.Create(value).Value
            ).IsRequired();

        builder.Property(u => u.Birthdate)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion
            (
                email => email!.Value,
                value => Email.Create(value).Value
            ).IsRequired();

        builder.OwnsOne(u => u.Address);

        builder.Property(u => u.States)
            .HasConversion
            (
                states => states!.ToString(),
                value => Enum.Parse<States>(value!)
            ).IsRequired();

        builder.Property(u => u.DateOfLastChange)
            .IsRequired();

        builder.HasOne(u => u.Profile)
            .WithMany()
            .HasForeignKey(u => u.ProfileId)
            .IsRequired();

        builder.Property<uint>("version")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}