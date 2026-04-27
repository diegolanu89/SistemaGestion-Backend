using bdt_evm_app.Data;
using bdt_evm_app.Middleware;
using bdt_evm_app.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration["AllowedOrigins"]?.Split(",")
            ?? ["http://localhost:3001"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

builder.Services.AddScoped<ClockifyService>();
builder.Services.AddScoped<ProjectBacService>();
builder.Services.AddScoped<ProjectMetricsService>();

var app = builder.Build();
var publicRoutes = new[] { "/api/auth/login", "/api/auth/login-with-profile", "/api/health", "/api/log-action" };

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseWhen(
    context => !publicRoutes.Any(route =>
        context.Request.Path.StartsWithSegments(route)),
    appBuilder => appBuilder.UseMiddleware<SanctumAuthMiddleware>()
);
app.MapControllers();
app.Run();
