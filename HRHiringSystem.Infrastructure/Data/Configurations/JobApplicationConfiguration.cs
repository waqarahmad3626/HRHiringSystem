using HRHiringSystem.Domain.Entities;
using HRHiringSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRHiringSystem.Infrastructure.Data.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(ja => ja.Id);

        builder.Property(ja => ja.Id).IsRequired();

        builder.Property(ja => ja.CandidateId).IsRequired();
        builder.HasOne(ja => ja.Candidate)
            .WithMany()
            .HasForeignKey(ja => ja.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ja => ja.JobId).IsRequired();

        builder.Property(ja => ja.CVUrl).IsRequired();

        builder.Property(ja => ja.Status)
            .IsRequired()
            .HasDefaultValue(ApplicationStatus.Pending)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ja => ja.Score)
            .IsRequired(false);

        builder.Property(ja => ja.MongoReportId)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(ja => ja.EvaluatedAt)
            .IsRequired(false);

        builder.Property(ja => ja.InterviewScheduledAt)
            .IsRequired(false);

        builder.Property(ja => ja.HRNotes)
            .IsRequired(false)
            .HasMaxLength(2000);

        // Indexes for common queries
        builder.HasIndex(ja => ja.Status);
        builder.HasIndex(ja => new { ja.CandidateId, ja.JobId }).IsUnique();

        // CreatedAt comes from AuditableEntity
        builder.Property(ja => ja.CreatedAt).IsRequired();
        builder.Property(ja => ja.IsDeleted).HasDefaultValue(false);
    }
}
