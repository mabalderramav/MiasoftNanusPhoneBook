using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiasoftNanus.PhoneBook.Infrastructure.Locations;

/// <summary>
/// Provides the configuration for the <see cref="Location"/> entity within the database context.
/// </summary>
/// <remarks>
/// This class configures the database schema for the <see cref="Location"/> entity,
/// including any property mappings, constraints, indexes, or relationships.
/// It implements the <see cref="IEntityTypeConfiguration{TEntity}"/> interface.
/// </remarks>
public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    void IEntityTypeConfiguration<Location>.Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(500);
        builder.HasMany(l => l.Agendas)
            .WithOne(a => a.Location)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property<uint>("version")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}