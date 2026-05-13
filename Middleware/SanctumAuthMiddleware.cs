using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;

namespace bdt_evm_app.Middleware;

public class SanctumAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SanctumAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var plainTextToken = context.Request.Cookies["auth_token"];

        if (string.IsNullOrEmpty(plainTextToken))
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "No autenticado"
            });

            return;
        }

        var parts = plainTextToken.Split('|');

        if (parts.Length != 2 || !ulong.TryParse(parts[0], out var tokenId))
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Token inválido"
            });

            return;
        }

        var rawToken = parts[1];

        var hashedToken = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawToken)
            )
        ).ToLower();

        var token = await db.PersonalAccessTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.Token == hashedToken);

        if (token == null)
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Token inválido"
            });

            return;
        }

        token.LastUsedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        context.Items["UserId"] = token.TokenableId;

        await _next(context);
    }
}