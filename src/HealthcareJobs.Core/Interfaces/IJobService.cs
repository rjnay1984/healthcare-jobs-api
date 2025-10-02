// src/HealthcareJobs.Core/Interfaces/IJobService.cs
using HealthcareJobs.Core.Entities;
using HealthcareJobs.Shared.DTOs;

namespace HealthcareJobs.Core.Interfaces;

public interface IJobService
{
    Task<JobPosting> CreateJobAsync(Guid employerId, CreateJobRequest request);
    Task<JobPosting?> GetJobByIdAsync(Guid jobId);
    Task<(List<JobPosting> jobs, int totalCount)> SearchJobsAsync(JobSearchRequest request);
    Task<JobPosting> UpdateJobAsync(Guid jobId, Guid employerId, UpdateJobRequest request);
    Task<bool> DeleteJobAsync(Guid jobId, Guid employerId);
    Task<JobApplication> ApplyToJobAsync(Guid jobId, Guid candidateId, JobApplicationRequest request);
    Task<bool> HasAlreadyAppliedAsync(Guid jobId, Guid candidateId);
}