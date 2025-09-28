using Microsoft.EntityFrameworkCore;

namespace HealthcareJobs.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    // Define your DbSets here. For example:
    // public DbSet<Job> Jobs { get; set; }
}