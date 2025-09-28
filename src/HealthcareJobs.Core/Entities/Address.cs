using System.ComponentModel.DataAnnotations;

namespace HealthcareJobs.Core.Entities;

public class Address : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = "United States";
}