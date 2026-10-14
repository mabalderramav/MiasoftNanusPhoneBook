using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiasoftNanus.PhoneBook.Infrastructure.WorkAreas;

/// <summary>
/// Configures the database schema for the <see cref="WorkArea"/> entity.
/// </summary>
/// <remarks>
/// The <c>WorkAreaConfiguration</c> class is responsible for defining the configuration of the
/// <see cref="WorkArea"/> entity within the database context. It is implemented as an Entity Framework Core
/// configuration class by inheriting from <see cref="IEntityTypeConfiguration{TEntity}"/>.
/// 
/// This configuration includes mapping and database-related settings, ensuring the <see cref="WorkArea"/> entity
/// is accurately represented in the database.
/// </remarks>
public class WorkAreaConfiguration : IEntityTypeConfiguration<WorkArea>
{
    void IEntityTypeConfiguration<WorkArea>.Configure(EntityTypeBuilder<WorkArea> builder)
    {
        builder.ToTable("work_areas");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(w => w.Description)
            .HasMaxLength(500);
        builder.HasMany(w => w.Workstations)
            .WithOne(u => u.WorkArea)
            .HasForeignKey(u => u.WorkAreaId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property<uint>("version")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}