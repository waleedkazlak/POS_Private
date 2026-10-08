using System.Reflection;
using System.Text;
using POS_Private.Application;
using POS_Private.Application.DTOs;
using POS_Private.Application.Services;
using POS_Private.Infrastructure;
using POS_Private.Infrastructure.Persistence;
using POS_Private.Infrastructure.Services;
using POS_Private.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

// Configure JWT Settings
var jwtSettings = new JwtSettingsDto
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? "POS_PrivatePosSecretKeyForJwtTokenAuthEgyptianBrand2026!",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "POS_PrivateAPI",
    Audience = builder.Configuration["Jwt:Audience"] ?? "POS_PrivateClients",
    ExpirationMinutes = int.TryParse(builder.Configuration["Jwt:ExpirationMinutes"], out var expMin) ? expMin : 120,
    RefreshTokenExpirationDays = int.TryParse(builder.Configuration["Jwt:RefreshTokenExpirationDays"], out var refDays) ? refDays : 30
};

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddSingleton(jwtSettings);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=POS_Private;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddInfrastructure(connectionString);

// Configure CORS for Angular and Android
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization
builder.Services.AddAuthorization();

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    var info = new OpenApiInfo
    {
        Version = "v1",
        Title = "POS Private API",
        Description = "Point of Sale (POS) and Master Data Management Web API for POS_Private Egyptian brand.",
        Contact = new OpenApiContact
        {
            Name = "POS Private Engineering",
            Email = "support@POS_Private.com"
        }
    };

    options.SwaggerDoc("v1", info);

    options.CustomSchemaIds(type =>
    {
        string GetSchemaName(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.Name;
            }

            var genericArgs = string.Join("_", t.GetGenericArguments().Select(GetSchemaName));
            var genericName = t.Name.Contains('`') ? t.Name[..t.Name.IndexOf('`')] : t.Name;
            return $"{genericName}_{genericArgs}";
        }

        return GetSchemaName(type);
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\nExample: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "POS Private API v1");
        options.DocumentTitle = "POS Private API Documentation";
        options.DefaultModelsExpandDepth(2);
        options.DisplayRequestDuration();
    });

    app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
}

app.UseCors("AllowAll");
app.UseExceptionHandling();
app.UseLanguageHeader();
app.UseStructuredLogging();

// Initialize database and seed data
try
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Checking database initialization and migrations...");

    var context = scope.ServiceProvider.GetRequiredService<POS_PrivateDbContext>();
    await context.Database.MigrateAsync();
    logger.LogInformation("Database migrated successfully.");

    await DatabaseSeeder.SeedAsync(context);
    logger.LogInformation("Database seeded successfully with POS_Private catalog.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database initialization / migration.");
    // In dev, let it continue or throw if needed
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Request Localization
var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("=== POS Private API Started ===");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("Default Credentials: admin / admin123 | manager / manager123 | cashier / cashier123");
startupLogger.LogInformation("===============================");

await app.RunAsync();
