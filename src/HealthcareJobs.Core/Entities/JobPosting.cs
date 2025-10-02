using System.ComponentModel.DataAnnotations;

using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Entities;

public class JobPosting : BaseEntity
{
    public Guid EmployerId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public YearsOfExperience MinExperience { get; set; }

    [Range(0, 10000000)]
    public decimal? SalaryMin { get; set; }

    [Range(0, 10000000)]
    public decimal? SalaryMax { get; set; }

    public bool IsRemote { get; set; }
    public bool RequiresLicense { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Active;

    // Navigation properties
    public Employer Employer { get; set; } = null!;
    public Address? JobLocation { get; set; }
    public List<Certification> RequiredCertifications { get; set; } = [];
    public List<Specialty> RequiredSpecialties { get; set; } = [];
    public List<JobApplication> JobApplications { get; set; } = [];
}