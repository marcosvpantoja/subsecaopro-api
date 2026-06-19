using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Amount).HasPrecision(10, 2);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(r => new { r.RoomId, r.Date, r.StartTime });

        builder.HasOne(r => r.Lawyer)
            .WithMany(l => l.Reservations)
            .HasForeignKey(r => r.LawyerId);

        builder.HasOne(r => r.Room)
            .WithMany(room => room.Reservations)
            .HasForeignKey(r => r.RoomId);

        builder.HasOne(r => r.Subsection)
            .WithMany()
            .HasForeignKey(r => r.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
