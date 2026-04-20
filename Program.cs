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

app.UseHttpsRedirection();
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/app"),
    appBuilder => appBuilder.UseMiddleware<SanctumAuthMiddleware>()
);
app.MapControllers();
app.Run();
