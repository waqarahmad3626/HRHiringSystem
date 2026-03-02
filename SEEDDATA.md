# Database Seeding Documentation

## Overview

TalentLink automatically initializes the database with migrations and seed data on the first startup. This allows users to immediately log in and test the system without manual setup.

## Automatic Seeding Process

When the API starts, it automatically:

1. **Applies Database Migrations** - Creates all required tables and schema
2. **Seeds Roles** - Creates Admin, HR, and Candidate roles
3. **Seeds Test Users** - Creates admin and HR test accounts
4. **Seeds Test Candidates** - Creates 3 sample job seekers
5. **Seeds Sample Jobs** - Creates 3 job postings ready for applications

## Seeded Data Details

### Roles

| Role Name | Description | Permissions |
|-----------|-------------|------------|
| **Admin** | Administrator with full system access | User/Job/Role management, System config |
| **HR** | Human Resources Manager | Application review, Report generation, Candidate viewing |
| **Candidate** | Job seeker | Job search, Application submission, Status tracking |

### Test Users

#### Admin Account
- **Email**: `test@capstone.com`
- **Password**: `Test1234!`
- **Name**: Test Admin
- **Role**: Admin
- **Purpose**: Complete system setup and management
- **Access**: All features (users, jobs, applications, reports)

#### HR Account
- **Email**: `hr@capstone.com`
- **Password**: `HR@Capstone123`
- **Name**: Test HR Manager
- **Role**: HR
- **Purpose**: Review applications and AI evaluations
- **Access**: Dashboard, Applications, Reports (read-only)

### Test Candidates

1. **John Smith**
   - Email: john.smith@example.com
   - Phone: 555-0101
   - Purpose: Sample applicant for testing

2. **Sarah Johnson**
   - Email: sarah.johnson@example.com
   - Phone: 555-0102
   - Purpose: Sample applicant for testing

3. **Michael Williams**
   - Email: michael.williams@example.com
   - Phone: 555-0103
   - Purpose: Sample applicant for testing

### Sample Jobs

#### 1. Senior Software Engineer
- **Description**: Experienced software engineer wanted for team
- **Requirements**:
  - 5+ years in C# and .NET
  - Cloud platforms (Azure/AWS)
  - Microservices architecture
  - SQL and NoSQL databases
  - Agile/Scrum experience
- **Created By**: Test HR Manager
- **Status**: Active

#### 2. Frontend Developer (Angular)
- **Description**: Talented Angular developer wanted
- **Requirements**:
  - 3+ years with Angular (v12+)
  - TypeScript proficiency
  - RESTful API integration
  - HTML/CSS/Bootstrap or Tailwind
  - Git version control
- **Created By**: Test HR Manager
- **Status**: Active

#### 3. Python Data Scientist
- **Description**: AI/ML Data Scientist wanted
- **Requirements**:
  - 3+ years with Python
  - TensorFlow or PyTorch experience
  - SQL knowledge
  - Statistics and ML basics
  - Google Cloud or AWS ML services
- **Created By**: Test HR Manager
- **Status**: Active

## Startup Logs

When the API starts, you'll see seed data initialization logs:

```
[APP] Initializing database...
[SEED] Database migrations applied successfully.
[SEED] Roles created successfully.
[SEED] Test users and candidates created:
  ✓ Admin User: test@capstone.com / Test1234!
  ✓ HR User: hr@capstone.com / HR@Capstone123
  ✓ Test Candidates: John Smith, Sarah Johnson, Michael Williams
[SEED] Sample jobs created:
  ✓ Senior Software Engineer
  ✓ Frontend Developer (Angular)
  ✓ Python Data Scientist
[SEED] Database initialization completed successfully.
```

## How Seeding Works

### Implementation Location

The seeding logic is in: `HRHiringSystem.Infrastructure/Data/SeedData.cs`

### Code Flow

```csharp
// In Program.cs startup
await SeedData.InitializeDatabaseAsync(app.Services);
  ├─ Apply migrations
  ├─ SeedRolesAsync()
  │   └─ Create Admin, HR, Candidate roles
  ├─ SeedTestDataAsync()
  │   ├─ Create Admin user (test@capstone.com)
  │   ├─ Create HR user (hr@capstone.com)
  │   ├─ Create 3 test candidates
  │   └─ Create 3 sample jobs
  └─ Complete
```

### Idempotent Design

The seeding is **idempotent**, meaning:
- It only runs on the FIRST startup when tables don't exist
- On subsequent startups, it checks if data exists and skips if already present
- You can safely restart the API without duplicating data

```csharp
var rolesExist = await dbContext.Roles.AnyAsync();
if (rolesExist) 
{
    Console.WriteLine("[SEED] Roles already exist, skipping...");
    return;
}
```

## Database Interaction

### First Startup Sequence

1. **No database exists**
   - ✅ Migrations create schema
   - ✅ Seed data inserted

2. **Database exists, no seed data**
   - ✅ Migrations apply new changes
   - ✅ Seed data inserted

3. **Database exists with seed data**
   - ✅ Migrations apply new changes
   - ⏭ Seed data skipped (already exists)

## Manual Seed Data Reset

If you need to reset seed data:

```bash
# Docker approach: Drop and recreate database
docker compose down -v
docker compose up --build -d

# Manual SQL approach
docker exec -it hrhiring-sqlserver sqlcmd -S localhost -U sa
# Then run:
# DROP DATABASE HRHiringSystemDB;
# GO
```

## Customizing Seed Data

To modify seed data:

1. Edit `HRHiringSystem.Infrastructure/Data/SeedData.cs`
2. Change the `SeedTestDataAsync()` method
3. Restart the API with fresh database:
   ```bash
   docker compose down -v
   docker compose up --build -d
   ```

### Example: Change Admin Password

```csharp
// In SeedTestDataAsync()
var adminPassword = "YourNewPassword123!"; // Change this
var adminPasswordHash = PasswordHasherService.HashPassword(adminPassword);

var adminUser = new User
{
    // ... other properties
    PasswordHash = adminPasswordHash,
};
```

## Security Considerations

⚠️ **Important for Production**:

1. **Change test passwords immediately** after first login
2. **Remove or disable test accounts** in production
3. **Modify seed data** before deploying to production
4. **Don't commit sensitive credentials** in the codebase

### Production Setup

For production deployment:

```csharp
// Disable seeding in production
if (app.Environment.IsDevelopment())
{
    await SeedData.InitializeDatabaseAsync(app.Services);
}
```

Or create a separate seeding CLI tool rather than auto-seeding.

## Troubleshooting

### Issue: Seed data not created

**Cause**: Database already exists with data
**Solution**: 
```bash
docker compose down -v  # Remove volumes
docker compose up --build -d
```

### Issue: Migration fails

**Cause**: Schema conflicts or invalid SQL
**Solution**: Check logs
```bash
docker compose logs api | grep -i error
```

### Issue: Duplicate data after restart

**Cause**: Seeding ran multiple times (shouldn't happen with idempotent design)
**Solution**: Verify seeding logic checks before inserting

```csharp
var rolesExist = await dbContext.Roles.AnyAsync();
if (rolesExist) return; // Safety check
```

## Related Files

- **Seed Implementation**: `HRHiringSystem.Infrastructure/Data/SeedData.cs`
- **Program Startup**: `HRHiringSystem.API/Program.cs` (lines ~220)
- **Entity Models**: `HRHiringSystem.Domain/Entities/`
- **DbContext**: `HRHiringSystem.Infrastructure/Data/AppDbContext.cs`

## Testing the Seed

### Quick Test

1. Start the API fresh: `docker compose down -v && docker compose up --build -d`
2. Wait for logs showing seed completion
3. Open http://localhost:4200
4. Login with `test@capstone.com` / `Test1234!`
5. Verify you can see the 3 sample jobs in the Dashboard

### Verify Seed Data

```bash
# Check roles
docker exec -it hrhiring-sqlserver sqlcmd -S localhost -U sa -P "YourPassword" -Q "SELECT * FROM Roles"

# Check users
docker exec -it hrhiring-sqlserver sqlcmd -S localhost -U sa -P "YourPassword" -Q "SELECT Email, Name FROM Users"

# Check jobs
docker exec -it hrhiring-sqlserver sqlcmd -S localhost -U sa -P "YourPassword" -Q "SELECT Title FROM Jobs"
```

---

**Last Updated**: March 2026  
**Related Files**: `Program.cs`, `SeedData.cs`
