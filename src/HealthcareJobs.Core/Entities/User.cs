using System.ComponentModel.DataAnnotations;

using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string AuthentikSubject { get; set; } = string.Empty;
    public UserType Type { get; set; }
    public bool IsActive { get; set; } = true;

}