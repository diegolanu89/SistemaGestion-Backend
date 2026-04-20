using bdt_evm_app.Data;
using bdt_evm_app.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    )
);

var app = builder.Build();

var publicRoutes = new[] { "/api/auth/login" };

app.UseHttpsRedirection();
app.UseWhen(
    context => !publicRoutes.Any(route =>
        context.Request.Path.StartsWithSegments(route)),
    appBuilder => appBuilder.UseMiddleware<SanctumAuthMiddleware>()
);
app.MapControllers();
app.Run();
