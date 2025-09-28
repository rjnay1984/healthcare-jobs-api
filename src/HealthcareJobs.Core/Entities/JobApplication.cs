using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Entities;

public class JobApplication : BaseEntity
{
    public Guid CandidateId { get; set; }
    public Guid JobPostingId { get; set; }
    public DateTime AppliedAt { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public string? CoverLetter { get; set; }
    
    // Navigation properties
    public Candidate Candidate { get; set; } = null!;
    public JobPosting JobPosting { get; set; } = null!;
}