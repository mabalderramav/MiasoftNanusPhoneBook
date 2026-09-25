using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiasoftNanus.PhoneBook.Infrastructure.Profiles;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    void IEntityTypeConfiguration<Profile>.Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("phonebook_profiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProfileName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(200);

        builder.Property<uint>("version")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}