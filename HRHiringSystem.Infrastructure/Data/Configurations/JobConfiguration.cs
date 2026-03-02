using HRHiringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRHiringSystem.Infrastructure.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id).IsRequired();

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(j => j.Description)
            .IsRequired();

        builder.Property(j => j.Requirements)
            .IsRequired();

        builder.Property(j => j.IsActive).HasDefaultValue(true);

        builder.Property(j => j.CreatedByHrId).IsRequired();

        builder.HasOne(j => j.CreatedByHr)
            .WithMany()
            .HasForeignKey(j => j.CreatedByHrId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.Applications)
            .WithOne(a => a.Job)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditableEntity
        builder.Property(j => j.CreatedAt).IsRequired();
        builder.Property(j => j.IsDeleted).HasDefaultValue(false);
    }
}
