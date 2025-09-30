
using HealthcareJobs.Core.Entities;
using HealthcareJobs.Shared.Enums;

using Microsoft.EntityFrameworkCore;

namespace HealthcareJobs.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Main entities
    public DbSet<User> Users { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Employer> Employers { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }

    // Reference data
    public DbSet<Certification> Certifications { get; set; }
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Candidate>(e => e.UserId) // Explicitly specify foreign key to avoid shadow property
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);

            // Many-to-many relationships
            entity.HasMany(e => e.Certifications)
                  .WithMany(e => e.Candidates);

            entity.HasMany(e => e.Specialties)
                  .WithMany(e => e.Candidates);
        });

        modelBuilder.Entity<Employer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employer>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NPINumber).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(500);
        });

        modelBuilder.Entity<JobPosting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Employer)
                .WithMany(e => e.JobPostings)
                .HasForeignKey(e => e.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.SalaryMin).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SalaryMax).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.PostedAt);
            entity.HasIndex(e => e.Status);

            // Many-to-many relationships
            entity.HasMany(e => e.RequiredCertifications)
                  .WithMany(e => e.JobPostings);
            entity.HasMany(e => e.RequiredSpecialties)
                  .WithMany(e => e.JobPostings);
        });

        // JobApplication configuration
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Candidate)
                .WithMany(e => e.JobApplications)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.JobPosting)
                .WithMany(e => e.JobApplications)
                .HasForeignKey(e => e.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CandidateId, e.JobPostingId }).IsUnique();
        });

        SeedReferenceData(modelBuilder);
    }

    private static void SeedReferenceData(ModelBuilder modelBuilder)
    {
        // Seed certifications
        modelBuilder.Entity<Certification>().HasData(
            new Certification { Id = 1, Name = "Epic Certified", Type = CertificationType.Technical },
            new Certification { Id = 2, Name = "Cerner Certified", Type = CertificationType.Technical },
            new Certification { Id = 3, Name = "CISSP", Type = CertificationType.Security },
            new Certification { Id = 4, Name = "CISA", Type = CertificationType.Security },
            new Certification { Id = 5, Name = "RN License", Type = CertificationType.Clinical },
            new Certification { Id = 6, Name = "RHIA", Type = CertificationType.Clinical }
        );

        // Seed specialties
        modelBuilder.Entity<Specialty>().HasData(
            new Specialty { Id = 1, Name = "Electronic Health Records", Category = SpecialtyCategory.Technical },
            new Specialty { Id = 2, Name = "Health Information Systems", Category = SpecialtyCategory.Technical },
            new Specialty { Id = 3, Name = "Clinical Data Analytics", Category = SpecialtyCategory.Technical },
            new Specialty { Id = 4, Name = "Cardiology", Category = SpecialtyCategory.Clinical },
            new Specialty { Id = 5, Name = "Radiology", Category = SpecialtyCategory.Clinical },
            new Specialty { Id = 6, Name = "Healthcare Administration", Category = SpecialtyCategory.Administrative }
        );
    }
}
