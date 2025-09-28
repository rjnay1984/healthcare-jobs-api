using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Entities;

public class Certification
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CertificationType Type { get; set; }
    
    // Navigation properties
    public List<Candidate> Candidates { get; set; } = [];
    public List<JobPosting> JobPostings { get; set; } = [];
}