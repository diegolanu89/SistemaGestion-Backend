## Migraciones SQL — RBAC normalizado y auditoría (RF-11)

Scripts manuales contra MariaDB/MySQL (motor `pm_clockify_evm`). El backend .NET no usa migraciones EF; estos archivos son la fuente de verdad para los cambios de schema aprobados.

Convención de naming: identificadores (tablas, columnas, codes) en **inglés snake_case**, matching el resto del schema (`profiles`, `users`, `personal_access_tokens`, etc.). Valores de display (`name`, `description`) en español por ser UI-facing.

### Orden de ejecución

| # | Archivo | Depende de |
|---|---|---|
| 1 | `001_create_modules.sql` | — |
| 2 | `002_create_actions.sql` | — |
| 3 | `003_create_permissions.sql` | 001 (FK `permissions.module_id`) |
| 4 | `004_create_profile_permissions.sql` | 003 + 002 + tabla `profiles` existente |
| 5 | `005_create_change_audit_log.sql` | — |

Todos los scripts son idempotentes (`CREATE TABLE IF NOT EXISTS`, `INSERT IGNORE` / `ON DUPLICATE KEY UPDATE`), pueden re-correrse sin efectos secundarios.

### Tablas creadas

| Tabla | Propósito | Filas seeded |
|---|---|---|
| `modules` | Taxonomía de módulos del sistema | 5 |
| `actions` | Lookup jerárquica de niveles de acceso | 3 |
| `permissions` | Catálogo de pantallas/recursos | 5 (1 por módulo) |
| `profile_permissions` | N:N profile↔permission con action_id | 13 |
| `change_audit_log` | Auditoría transversal RF-11 | 0 (se llena por interceptor) |

### Decisiones de diseño relevantes

- **Modelo de acciones jerárquico**, no atómico. La tabla `actions` define 3 niveles inclusivos:
  - `read_only` (level 1) → ver
  - `edit` (level 2) → ver + editar
  - `create` (level 3) → ver + editar + crear + eliminar (full)

  El check de autorización es `current_level >= required_level`. Si en el futuro aparece una acción nueva que encaja en la jerarquía (ej. "approve" entre `edit` y `create`), se inserta en la tabla sin migrar código.
- **`action_id` vive en `profile_permissions`, no en `permissions`** — la acción es propiedad de la asignación profile↔permission, no del recurso. `permissions` es puro catálogo.
- **PK compuesta en `profile_permissions`** — `(profile_id, permission_id)`. Un único nivel por par.
- **FK `profile_id` (no `role_id`)** — la tabla `profiles` ES la tabla de roles en este codebase; matchea la convención existente de `users.profile_id`.
- **Default de implementación** — todos los profiles arrancan con `create` (full access) sobre los módulos asignados. Restricciones más finas se definen luego, sin retrofit estructural.
- **Seed de `permissions`** — 1 fila por módulo (5 baselines, code = `<module_code>`). Permisos a nivel pantalla se agregan luego con code `<module>.<screen>`.
- **Seed de `profile_permissions`** — replica el mapa hardcodeado de `Services/ProfileCatalog.cs`, todo a nivel `create`. Preserva el comportamiento exacto al cutover.
- **`change_audit_log`** — sin FK sobre `user_id` ni `module` (decoupling para performance — constraint exigido por Sergio en la aprobación). Índices pensados para los tres accesos previstos: histórico por registro, actividad por usuario, y sweeps por timestamp.
- **`change_audit_log.event_type`** — ENUM(`create`,`update`,`delete`,`login`,`logout`). Tipo de evento auditado. Columna llamada `event_type` (no `action`) para no chocar con la tabla `actions` de RBAC, que es un concepto distinto.

### Cutover por etapas — `users.screen_permissions`

La columna `users.screen_permissions` (longtext JSON) **no se dropea en estas migraciones**. Queda como nullable obsoleta hasta que:

1. El backend .NET reemplace `Services/ProfileCatalog.cs` por queries contra `profile_permissions JOIN actions`.
2. `AuthController` retorne `permissions: [{code, action, level}]` desde la BD (no desde `ProfileCatalog`).
3. El front consuma el nuevo payload y deje de leer `screen_permissions`.

Recién entonces se agrega un `006_drop_users_screen_permissions.sql`.

### Validación post-ejecución

```sql
-- 1) 5 modules
SELECT COUNT(*) FROM modules;                          -- expected: 5

-- 2) 3 hierarchical actions
SELECT code, level FROM actions ORDER BY level;        -- expected: read_only=1, edit=2, create=3

-- 3) 5 baseline permissions (one per module)
SELECT COUNT(*) FROM permissions;                      -- expected: 5

-- 4) 13 rows in profile_permissions (admin 5 + administracion 2 + ops_gerente 3 + ops_lider 2 + soporte 1)
SELECT COUNT(*) FROM profile_permissions;              -- expected: 13

-- 5) Map per profile with current level
SELECT pf.code AS profile, ac.code AS action, COUNT(*) AS perms
FROM profile_permissions pp
JOIN profiles pf ON pf.id = pp.profile_id
JOIN actions ac ON ac.id = pp.action_id
GROUP BY pf.code, ac.code
ORDER BY pf.code;
-- expected: every profile with action='create'
--           admin=5, administracion=2, ops_gerente=3, ops_lider=2, soporte=1

-- 6) Audit log empty and indexed
SELECT COUNT(*) FROM change_audit_log;                 -- expected: 0
SHOW INDEX FROM change_audit_log;                      -- expected: 4 idx_audit_*
```
