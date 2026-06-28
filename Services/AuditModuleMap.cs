namespace bdt_evm_app.Services;

// Mapea entidad EF (ClrType.Name) → código de módulo ERS.
// Códigos válidos: operations, analysis, reports, administration, configuration.
//
// Si una entidad no está en el mapa, el interceptor persiste module = NULL.
// Eso es preferible a inventar un módulo: el operador del audit screen ve
// "sin módulo" y el equipo decide después dónde clasificarla.
public static class AuditModuleMap
{
    private static readonly Dictionary<string, string> _map = new(StringComparer.Ordinal)
    {
        // Administración — usuarios, perfiles, calendarios, RBAC
        ["User"]                 = "administration",
        ["Profile"]              = "administration",
        ["UserLeader"]           = "administration",
        ["UserVacationPeriod"]   = "administration",
        ["UserMonthlyCapacity"]  = "administration",
        ["WorkingDaysCalendar"]  = "administration",
        ["Module"]               = "administration",
        ["Permission"]           = "administration",
        ["PermissionAction"]     = "administration",
        ["ProfilePermission"]    = "administration",

        // Operación — proyectos, horas, ETC, intake
        ["TimesheetClient"]               = "operations",
        ["TimesheetProject"]              = "operations",
        ["TimesheetProjectFilter"]        = "operations",
        ["TimesheetTimeEntry"]            = "operations",
        ["TimesheetUser"]                 = "operations",
        ["ChangeRequest"]                = "operations",
        ["EtcSnapshot"]                  = "operations",
        ["EtcRecord"]                    = "operations",
        ["ProjectIntakeRecord"]          = "operations",
        ["ProjectIntakeTypeRef"]         = "operations",
        ["ProjectIntakeCategoryRef"]     = "operations",
        ["ProjectIntakeStatusRef"]       = "operations",
        ["ProjectTracking"]              = "operations",
        ["ProjectTrackingUpdate"]        = "operations",
        ["PotencialClient"]              = "operations",
        ["PotencialProject"]             = "operations",
        ["PotencialProjectAllocation"]   = "operations",

        // Configuración — preferencias por usuario, visibilidad
        ["AppUserVisibleProject"] = "configuration",
        ["UserDashboardFilter"]   = "configuration",
    };

    public static string? Resolve(string entityName)
        => _map.TryGetValue(entityName, out var module) ? module : null;
}
