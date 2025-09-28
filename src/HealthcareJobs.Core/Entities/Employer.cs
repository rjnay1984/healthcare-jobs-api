using System.ComponentModel.DataAnnotations;

using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Entities;

public class Employer : BaseEntity
{
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    
    public HealthcareOrganizationType OrganizationType { get; set; }
    
    [StringLength(20)]
    public string? NPINumber { get; set; }
    
    public bool IsHIPAACompliant { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Url]
    public string? Website { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Address? CompanyAddress { get; set; }
    public List<JobPosting> JobPostings { get; set; } = [];
}