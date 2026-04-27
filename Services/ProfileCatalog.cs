namespace bdt_evm_app.Services;

/// <summary>
/// Catálogo estático de perfiles reconocidos y su mapa de permisos por módulo.
/// Punto de extensión: cuando exista la tabla PERMISOS / PERMISOS_ROL en BD,
/// reemplazar el contenido de GetPermissions por una query, manteniendo la firma.
/// </summary>
public static class ProfileCatalog
{
    public const string Admin = "admin";
    public const string Soporte = "soporte";
    public const string OpsGerente = "ops_gerente";
    public const string OpsLider = "ops_lider";
    public const string Administracion = "administracion";

    public static readonly IReadOnlySet<string> RecognizedCodes = new HashSet<string>
    {
        Admin,
        Soporte,
        OpsGerente,
        OpsLider,
        Administracion
    };

    public const string ModuleOperations = "module:operations";
    public const string ModuleAnalysis = "module:analysis";
    public const string ModuleReports = "module:reports";
    public const string ModuleAdministration = "module:administration";
    public const string ModuleConfiguration = "module:configuration";

    private static readonly Dictionary<string, IReadOnlyList<string>> _map = new()
    {
        [Admin] = new[]
        {
            ModuleOperations, ModuleAnalysis, ModuleReports,
            ModuleAdministration, ModuleConfiguration
        },
        [Administracion] = new[] { ModuleAdministration, ModuleConfiguration },
        [OpsGerente] = new[] { ModuleOperations, ModuleAnalysis, ModuleReports },
        [OpsLider] = new[] { ModuleOperations, ModuleAnalysis },
        [Soporte] = new[] { ModuleReports }
    };

    public static bool IsRecognized(string? code) =>
        !string.IsNullOrEmpty(code) && RecognizedCodes.Contains(code);

    public static IReadOnlyList<string> GetPermissions(string? code) =>
        code != null && _map.TryGetValue(code, out var perms)
            ? perms
            : Array.Empty<string>();
}
