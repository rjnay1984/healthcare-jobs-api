using System.ComponentModel.DataAnnotations;

using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Entities;

public class Candidate : BaseEntity
{
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public required string LastName { get; set; }

    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    public LicenseType? LicenseType { get; set; }
    public DateTime? LicenseExpiry { get; set; }
    public YearsOfExperience ExperienceLevel { get; set; }
    public bool WillRelocate { get; set; }

    [Range(0, 1000000)]
    public decimal? DesiredSalaryMin { get; set; }

    [Url]
    public string? ResumeUrl { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public List<Certification> Certifications { get; set; } = [];
    public List<Specialty> Specialties { get; set; } = [];
    public List<JobApplication> JobApplications { get; set; } = [];
}