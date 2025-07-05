using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeaBattle.Backend.Application.DependencyInjection;
using SeaBattle.Backend.Infrastructure.Data;
using SeaBattle.Backend.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Для JWT
using Microsoft.IdentityModel.Tokens;             // Для JWT
using System.Text;                                // Для Encoding.UTF8
using SeaBattle.Backend.Domain.Configuration;     // Для JwtSettings
using Microsoft.Extensions.Options;               // Для IOptions (если используется в других местах)
using Microsoft.AspNetCore.Cors.Infrastructure;   // Для Cors (если используется)

// Настройка Serilog для раннего логирования (до создания хоста)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Использование Serilog для логирования внутренних процессов ASP.NET Core
    builder.Host.UseSerilog();

    // --- Добавление сервисов в контейнер ---
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "SeaBattle Backend API",
            Version = "v1",
            Description = "API для игры \"Морской бой\", предоставляющий функционал для управления пользователями и игровыми сессиями."
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
        else
        {
            Log.Warning("XML Documentation file not found at {XmlPath}. Swagger documentation might be incomplete.", xmlPath);
        }
    });

    // ************************************************************
    // ИСПРАВЛЕНИЯ И ДОБАВЛЕНИЯ В builder.Services
    // ************************************************************

    // Добавляем HttpContextAccessor для доступа к HttpContext в сервисах (для получения IP-адреса)
    builder.Services.AddHttpContextAccessor(); // <-- ДОБАВЛЕНО!

    // Настройка JWT Authentication
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.Jwt).Get<JwtSettings>();
    if (jwtSettings == null)
    {
        Log.Fatal("JwtSettings не найдены в конфигурации или не могут быть десериализованы. Проверьте appsettings.json.");
        throw new InvalidOperationException("JwtSettings не найдены в конфигурации.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
        // Для логирования проблем с токенами во время разработки
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error(context.Exception, "Authentication failed.");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("Token successfully validated.");
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization(); // <-- ДОБАВЛЕНО!

    // Настройка CORS (Cross-Origin Resource Sharing)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            policyBuilder => policyBuilder.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:8080") // Добавь сюда домены своего фронтенда
                                         .AllowAnyHeader()
                                         .AllowAnyMethod()
                                         .AllowCredentials());
    });

    // Регистрация сервисов слоев
    builder.Services.AddInfrastructureServices(builder.Configuration); // Должен быть перед Application, если Application зависит от Infrastructure
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddRecaptchaServices(builder.Configuration);

    // --- Настройка Health Checks ---
    builder.Services
        .AddHealthChecks()
        .AddDbContextCheck<SeaBattleDbContext>(
            name: "postgresql",
            tags: new[] { "db", "ready" })
        .AddCheck("liveness", () =>
            HealthCheckResult.Healthy("OK"), tags: ["live"]);

    var app = builder.Build();
    
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SeaBattleDbContext>();

        if (app.Environment.IsDevelopment())
        {
            Log.Information("Applying migrations for SeaBattleDbContext...");
            await dbContext.Database.MigrateAsync();
            Log.Information("Migrations applied successfully.");
        }

        var canConnect = await dbContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            Log.Fatal("Database connection failed. The application cannot start without a database connection.");
            throw new InvalidOperationException("DB not available");
        }
        Log.Information("Successfully connected to the database.");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SeaBattle Backend API v1");
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors("AllowSpecificOrigin");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health Checks mappings
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("live"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description, exception = e.Value.Exception?.Message })
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}