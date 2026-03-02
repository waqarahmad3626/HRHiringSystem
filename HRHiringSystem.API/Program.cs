using HRHiringSystem.Infrastructure.Data;
using HRHiringSystem.Application.Interfaces;
using Azure.Storage.Blobs;
using HRHiringSystem.Infrastructure.Storage;
using HRHiringSystem.Application.DependencyInjection;
using HRHiringSystem.Application.Mappings;
using Microsoft.EntityFrameworkCore;
using HRHiringSystem.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using HRHiringSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "http://127.0.0.1:4200" };

// Add services to the container.
// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HRHiringSystem.API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// Register application validators from the Application assembly
builder.Services.AddApplicationValidation();
// Add controllers and enable controller routing
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// AutoMapper - register application mapping profiles
builder.Services.AddApplicationMappings();

// Repositories
builder.Services.AddInfrastructureRepositories();

// MongoDB for evaluation reports
builder.Services.AddMongoDb(builder.Configuration);

// AI Agent HTTP client for CV parsing and evaluation
builder.Services.AddAIAgentClient(builder.Configuration);

// Azure Function HTTP client for background processing
builder.Services.AddAzureFunctionClient(builder.Configuration);

// Handlers
builder.Services.AddApplicationHandlers();

// Blob storage service - use Azure if configured, otherwise use local file storage
var blobConn = builder.Configuration.GetConnectionString("AzureBlob");
if (!string.IsNullOrEmpty(blobConn))
{
    builder.Services.AddSingleton<IBlobStorageService>(sp =>
    {
        var client = new BlobServiceClient(blobConn);
        return new AzureBlobStorageService(client);
    });
}
else
{
    // Fallback to local file storage for development
    var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
    var baseUrl = builder.Configuration["BaseUrl"] ?? "http://localhost:8080";
    builder.Services.AddSingleton<IBlobStorageService>(sp => new LocalFileStorageService(uploadsPath, baseUrl));
}

// Redis ConnectionMultiplexer
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var mux = ConnectionMultiplexer.Connect(redisConn);
builder.Services.AddSingleton<IConnectionMultiplexer>(mux);

// Token service (infrastructure implementation) - registered in infra project via DI, but register here if needed
builder.Services.AddScoped<ITokenService, TokenService>();

// Authentication JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "please-change-this-secret";
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
            ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true
        };

        // Reject tokens which are not present in redis (e.g., logged out)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                if (!string.IsNullOrEmpty(token)) context.Token = token;
                Console.WriteLine($"[JWT] OnMessageReceived: Token present = {!string.IsNullOrEmpty(token)}");
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                Console.WriteLine($"[JWT] OnTokenValidated: Token = {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("[JWT] OnTokenValidated: FAIL - No token");
                    context.Fail("No token");
                    return;
                }

                var muxer = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                var db = muxer.GetDatabase();
                var exists = await db.KeyExistsAsync($"tokens:{token}");
                Console.WriteLine($"[JWT] OnTokenValidated: Redis key exists = {exists}");
                if (!exists) 
                {
                    Console.WriteLine("[JWT] OnTokenValidated: FAIL - Token not active");
                    context.Fail("Token not active");
                }
                else
                {
                    Console.WriteLine("[JWT] OnTokenValidated: SUCCESS");
                }
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT] OnAuthenticationFailed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRHiringSystem.API v1");
    });
}

// Global exception handling middleware
app.UseMiddleware<HRHiringSystem.API.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Serve uploaded files from local storage (for development)
var staticUploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(staticUploadsPath))
{
    Directory.CreateDirectory(staticUploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(staticUploadsPath),
    RequestPath = "/uploads"
});

// Configure routing and authorization for controllers
app.UseRouting();
app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Initialize database with migrations and seed data
Console.WriteLine("[APP] Initializing database...");
await SeedData.InitializeDatabaseAsync(app.Services);

app.Run();

