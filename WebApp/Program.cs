using Application.Chat;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using WebApp.Extensions;
using WebApp.Hubs;
using WebApp.Middleware;
using AspNetCoreRateLimit;
using Infrastructure.Chat;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("AspNetCoreRateLimit", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SignalRDemo")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/signalr-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting SignalR Chat application");

    var builder = WebApplication.CreateBuilder(args);

    // CORS 
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.SetIsOriginAllowed(origin => true) 
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();                  
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddScoped<IFriendService, FriendService>();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SignalR Chat API",
            Version = "v1"
        });

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Description = "Format: Bearer {token}",
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme
            }
        };

        c.AddSecurityDefinition("Bearer", jwtSecurityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtSecurityScheme, Array.Empty<string>() }
        });
    });

    builder.Host.UseSerilog();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Log.Information("Checking for pending database migrations...");
        var pending = dbContext.Database.GetPendingMigrations().ToList();
        if (pending.Any())
        {
            Log.Information("Applying migrations: {Migrations}", string.Join(", ", pending));
            dbContext.Database.Migrate();
            Log.Information("Migrations applied successfully");
        }
        else
        {
            Log.Information("Database is up to date");
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalR Chat API v1");
        c.RoutePrefix = "swagger";           
        c.DisplayOperationId();              
        c.DisplayRequestDuration();         
    });

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseIpRateLimiting();
    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCors("AllowAll");

    app.MapControllers();
    app.MapHub<ChatHub>("/chatHub").RequireCors("AllowAll");

    Log.Information("Application configured successfully. Listening on {Urls}", 
        string.Join(", ", builder.Configuration.GetValue<string>("ASPNETCORE_URLS") ?? "http://localhost:1111"));
    
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