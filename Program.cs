using bdt_evm_app.Data;
using bdt_evm_app.Middleware;
using bdt_evm_app.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SistemaGestion API", Version = "v1" });
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Name = "auth_token",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Description = "Autenticación por cookie. Llamá a POST /api/auth/login primero."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "cookieAuth" }
            },
            []
        }
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

builder.Services.AddScoped<ClockifyService>();
builder.Services.AddScoped<ProjectBacService>();
builder.Services.AddScoped<ProjectMetricsService>();
builder.Services.AddScoped<ProjectIntakeService>();

var app = builder.Build();
var publicRoutes = new[] { "/api/auth/login", "/api/health", "/api/log-action", "/swagger" };

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseWhen(
    context => !publicRoutes.Any(route =>
        context.Request.Path.StartsWithSegments(route)),
    appBuilder => appBuilder.UseMiddleware<SanctumAuthMiddleware>()
);
app.MapControllers();
app.Run();
