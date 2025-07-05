using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeaBattle.Backend.Infrastructure.Data;
using SeaBattle.Backend.Infrastructure.DependencyInjection;

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

    // --- Настройка Swagger/OpenAPI ---
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

    // --- Регистрация инфраструктурных сервисов ---
    builder.Services.AddInfrastructureServices(builder.Configuration);
    
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

    // --- Применение миграций и проверка соединения с БД при запуске ---
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

    app.UseAuthorization();

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

    app.MapControllers();

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