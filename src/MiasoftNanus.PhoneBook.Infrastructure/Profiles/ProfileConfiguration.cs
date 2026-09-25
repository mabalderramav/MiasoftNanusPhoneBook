using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiasoftNanus.PhoneBook.Infrastructure.Profiles;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
         builder.ToTable("profiles");
         builder.HasKey(r => r.Id); 

         builder.Property(r => r.ProfileName)
                .IsRequired()
                .HasMaxLength(50);

        builder.Property(r => r.Description)
                .HasMaxLength(200);

        builder.Property<uint>("version")
                .IsRowVersion()
                .IsConcurrencyToken();
    }
}