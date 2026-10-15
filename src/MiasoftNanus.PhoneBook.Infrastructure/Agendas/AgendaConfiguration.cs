using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Shared.ObjectValues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiasoftNanus.PhoneBook.Infrastructure.Agendas;

/// <summary>
/// Configures the database schema for the <see cref="Agenda"/> entity.
/// </summary>
/// <remarks>
/// The <see cref="AgendaConfiguration"/> class implements the <see cref="IEntityTypeConfiguration{TEntity}"/>
/// interface to define database-specific configurations for the <see cref="Agenda"/> entity. This includes
/// configuring properties, constraints, relationships, and other schema-related details.
/// </remarks>
public class AgendaConfiguration : IEntityTypeConfiguration<Agenda>
{
    void IEntityTypeConfiguration<Agenda>.Configure(EntityTypeBuilder<Agenda> builder)
    {
        builder.ToTable("agendas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(a => a.Email)
            .HasConversion
            (
                email => email!.Value,
                value => Email.Create(value).Value
            ).IsRequired();
        builder.Property(a => a.Enabled)
            .IsRequired();
        builder.Property(a => a.ImagePath)
            .HasMaxLength(300);
        builder.Property(a => a.DirectLine)
            .HasMaxLength(50);
        builder.Property(a => a.Internal)
            .HasMaxLength(50);
        builder.Property(a => a.Corporate)
            .HasMaxLength(50);
        builder.Property(a => a.DirectAccess)
            .HasMaxLength(50);
        builder.Property(a => a.WorkstationId)
            .IsRequired();
        builder.HasOne(a => a.Workstation)
            .WithMany(w => w.Agendas)
            .HasForeignKey(a => a.WorkstationId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(a => a.LocationId)
            .IsRequired();
        builder.HasOne(a => a.Location)
            .WithMany(l => l.Agendas)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(a => a.UserIdCreated)
            .IsRequired();
        builder.HasOne(a => a.UserCreated)
            .WithMany(u => u.Agendas)
            .HasForeignKey(a => a.UserIdCreated)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property<uint>("version")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}