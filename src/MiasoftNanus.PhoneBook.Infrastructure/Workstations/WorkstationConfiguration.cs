using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiasoftNanus.PhoneBook.Infrastructure.Workstations;

/// <summary>
/// Configures the database schema for the <see cref="Workstation"/> entity type.
/// </summary>
/// <remarks>
/// This class is responsible for defining the structure, relationships, constraints,
/// and any additional configuration necessary for persisting <see cref="Workstation"/>
/// entities in the database. It is used by the Entity Framework Core to build the
/// model during the migration and runtime processes.
/// </remarks>
public class WorkstationConfiguration : IEntityTypeConfiguration<Workstation>
{
    void IEntityTypeConfiguration<Workstation>.Configure(EntityTypeBuilder<Workstation> builder)
    {
        builder.ToTable("workstations");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(w => w.Description)
            .HasMaxLength(500);
        builder.Property(w => w.WorkAreaId)
            .IsRequired();
        builder.HasOne(w => w.WorkArea)
            .WithMany(wa => wa.Workstations)
            .HasForeignKey(w => w.WorkAreaId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property<uint>("version")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}