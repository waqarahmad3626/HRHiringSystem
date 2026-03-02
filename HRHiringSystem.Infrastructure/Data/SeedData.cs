using HRHiringSystem.Application.Helpers;
using HRHiringSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRHiringSystem.Infrastructure.Data;

/// <summary>
/// Seed data helper for initializing database with default roles and test users
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Apply migrations and seed initial data on application startup
    /// </summary>
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Apply any pending migrations
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("[SEED] Database migrations applied successfully.");

            // Seed roles (if not already seeded)
            await SeedRolesAsync(dbContext);

            // Seed users and test data
            await SeedTestDataAsync(dbContext);

            Console.WriteLine("[SEED] Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEED] ERROR: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Create default roles (Admin, HR, Candidate)
    /// </summary>
    private static async Task SeedRolesAsync(AppDbContext dbContext)
    {
        var rolesExist = await dbContext.Roles.AnyAsync();
        if (rolesExist)
        {
            Console.WriteLine("[SEED] Roles already exist, skipping...");
            return;
        }

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Admin",
                Description = "Administrator with full system access"
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "HR",
                Description = "HR Manager who reviews and manages applications"
            },
            new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Candidate",
                Description = "Job candidate who applies for positions"
            }
        };

        await dbContext.Roles.AddRangeAsync(roles);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("[SEED] Roles created successfully.");
    }

    /// <summary>
    /// Create test admin user and sample data
    /// </summary>
    private static async Task SeedTestDataAsync(AppDbContext dbContext)
    {
        var usersExist = await dbContext.Users.AnyAsync();
        if (usersExist)
        {
            Console.WriteLine("[SEED] Users already exist, skipping test data...");
            return;
        }

        var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var hrRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var candidateRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Create test admin user
        var adminPassword = "Test1234!";
        var adminPasswordHash = PasswordHasherService.HashPassword(adminPassword);

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test Admin",
            Email = "test@capstone.com",
            PasswordHash = adminPasswordHash,
            RoleId = adminRoleId,
            IsActive = true
        };

        // Create test HR user
        var hrUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test HR Manager",
            Email = "hr@capstone.com",
            PasswordHash = PasswordHasherService.HashPassword("HR@Capstone123"),
            RoleId = hrRoleId,
            IsActive = true
        };

        // Create test candidates
        var candidate1 = new Candidate
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@example.com",
            PhoneNumber = "555-0101"
        };

        var candidate2 = new Candidate
        {
            Id = Guid.NewGuid(),
            FirstName = "Sarah",
            LastName = "Johnson",
            Email = "sarah.johnson@example.com",
            PhoneNumber = "555-0102"
        };

        var candidate3 = new Candidate
        {
            Id = Guid.NewGuid(),
            FirstName = "Michael",
            LastName = "Williams",
            Email = "michael.williams@example.com",
            PhoneNumber = "555-0103"
        };

        // Add users and candidates to context
        await dbContext.Users.AddAsync(adminUser);
        await dbContext.Users.AddAsync(hrUser);
        await dbContext.Candidates.AddRangeAsync(candidate1, candidate2, candidate3);
        await dbContext.SaveChangesAsync();

        Console.WriteLine("[SEED] Test users and candidates created:");
        Console.WriteLine($"  ✓ Admin User: test@capstone.com / Test1234!");
        Console.WriteLine($"  ✓ HR User: hr@capstone.com / HR@Capstone123");
        Console.WriteLine($"  ✓ Test Candidates: John Smith, Sarah Johnson, Michael Williams");

        // Create sample jobs
        var sampleJobs = new List<Job>
        {
            new Job
            {
                Id = Guid.NewGuid(),
                Title = "Senior Software Engineer",
                Description = "We are looking for an experienced software engineer to join our team.",
                Requirements = "5+ years of experience in C# and .NET\nExperience with cloud platforms (Azure/AWS)\nKnowledge of microservices architecture\nSQL and NoSQL databases\nAgile/Scrum experience",
                CreatedByHrId = hrUser.Id,
                IsActive = true
            },
            new Job
            {
                Id = Guid.NewGuid(),
                Title = "Frontend Developer (Angular)",
                Description = "Seeking a talented Frontend Developer with strong Angular skills.",
                Requirements = "3+ years with Angular (v12+)\nTypeScript proficiency\nRESTful API integration\nHTML/CSS/Bootstrap or Tailwind\nGit version control",
                CreatedByHrId = hrUser.Id,
                IsActive = true
            },
            new Job
            {
                Id = Guid.NewGuid(),
                Title = "Python Data Scientist",
                Description = "Join our AI/ML team as a Data Scientist.",
                Requirements = "3+ years with Python\nExperience with TensorFlow or PyTorch\nSQL knowledge\nStatistics and machine learning basics\nExperience with Google Cloud or AWS ML services",
                CreatedByHrId = hrUser.Id,
                IsActive = true
            }
        };

        await dbContext.Jobs.AddRangeAsync(sampleJobs);
        await dbContext.SaveChangesAsync();

        Console.WriteLine("[SEED] Sample jobs created:");
        foreach (var job in sampleJobs)
        {
            Console.WriteLine($"  ✓ {job.Title}");
        }
    }
}
