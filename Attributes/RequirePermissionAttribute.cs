using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;

namespace bdt_evm_app.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _permissionCodes;

    public RequirePermissionAttribute(params string[] permissionCodes)
    {
        _permissionCodes = permissionCodes;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;

        if (http.Items["UserId"] is not ulong userId)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "No autenticado" });
            return;
        }

        var db = http.RequestServices.GetRequiredService<AppDbContext>();

        var profileId = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.ProfileId)
            .FirstOrDefaultAsync();

        if (profileId is null)
        {
            context.Result = new ObjectResult(new { message = "Usuario sin perfil asignado" })
            {
                StatusCode = 403
            };
            return;
        }

        var allowed = await db.ProfilePermissions
            .AnyAsync(pp =>
                pp.ProfileId == profileId.Value &&
                _permissionCodes.Contains(pp.Permission!.Code));

        if (!allowed)
        {
            var required = string.Join(" | ", _permissionCodes);
            context.Result = new ObjectResult(new { message = $"Permiso requerido: {required}" })
            {
                StatusCode = 403
            };
            return;
        }
    }
}
