using bdt_evm_app.DTOs;

namespace bdt_evm_app.Services;

/// <summary>
/// Abstracción del proveedor de time tracking. Devuelve DTOs neutros ya mapeados,
/// de modo que la lógica de negocio no conoce el formato (shape) del proveedor concreto.
/// Implementación actual: <see cref="ClockifyProvider"/>.
/// </summary>
public interface ITimesheetProvider
{
    Task<IReadOnlyList<TimesheetClientDto>> GetClientsAsync();
    Task<IReadOnlyList<TimesheetUserDto>> GetUsersAsync(bool onlyActive = false);
    Task<IReadOnlyList<TimesheetProjectDto>> GetProjectsAsync();
    Task<IReadOnlyList<TimesheetEntryDto>> GetEntriesForUserAsync(string userExternalId, string? start, string? end, int page, int pageSize);
    Task<IReadOnlyList<TimesheetEntryDto>> GetEntriesForProjectAsync(string projectExternalId, string? start, string? end, int page, int pageSize);
    IAsyncEnumerable<TimesheetEntryDto> GetAllEntriesAsync(string? start, string? end);

    /// <summary>
    /// Id del usuario por defecto del proveedor (ej. CLOCKIFY_USER_ID).
    /// Puede ser vacío/nulo si no está configurado.
    /// </summary>
    string? DefaultUserExternalId { get; }
}
