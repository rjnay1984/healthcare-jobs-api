// src/HealthcareJobs.Shared/Enums/JobStatus.cs
namespace HealthcareJobs.Shared.Enums;

public enum JobStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Filled = 3,
    Cancelled = 4,
    Expired = 5
}