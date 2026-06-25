-- ============================================================
-- 007_seed_rbac_data.sql
-- Seed del sistema RBAC: módulos, acciones, permisos y
-- asignaciones por perfil.
-- Idempotente:
--   - modules / actions: INSERT IGNORE
--   - permissions / profile_permissions: DELETE + re-INSERT
--     (los códigos de permiso son la fuente de verdad;
--      los IDs auto-increment pueden variar entre instancias)
-- Requiere que migrations 003 ya haya creado las tablas.
-- ============================================================

USE pm_timesheet_evm;

-- ── Módulos ──────────────────────────────────────────────────
INSERT IGNORE INTO `modules` (`code`, `name`, `description`, `active`, `created_at`, `updated_at`)
VALUES
  ('PROJECTS',           'Gestión de Proyectos',   'Pantallas del módulo Operación — proyectos',          1, NOW(), NOW()),
  ('ETC',                'Carga ETC',              'Pantallas del módulo Operación — carga ETC',          1, NOW(), NOW()),
  ('ESTIMATED_PROJECTS', 'Proyectos Estimados',    'Pantallas del módulo Operación — proyectos estimados',1, NOW(), NOW()),
  ('DASHBOARD',          'Dashboards',             'Pantallas del módulo Análisis',                       1, NOW(), NOW()),
  ('REPORTS',            'Reportería',             'Pantallas del módulo Reportería',                     1, NOW(), NOW()),
  ('ADMINISTRATION',     'Administración',         'Pantallas del módulo Administración',                 1, NOW(), NOW()),
  ('SETTINGS',           'Configuración',          'Pantallas del módulo Configuración',                  1, NOW(), NOW());

-- ── Acciones (niveles jerárquicos) ───────────────────────────
INSERT IGNORE INTO `actions` (`code`, `name`, `description`, `level`, `active`, `created_at`, `updated_at`)
VALUES
  ('read_only', 'Sólo lectura', 'Permite ver la pantalla. No puede editar, crear ni eliminar.', 1, 1, NOW(), NOW()),
  ('edit',      'Edición',      'Permite ver y editar registros existentes.',                   2, 1, NOW(), NOW()),
  ('create',    'Creación',     'Acceso total: ver, editar, crear y eliminar.',                 3, 1, NOW(), NOW());

-- ── Permisos ─────────────────────────────────────────────────
-- Eliminar y re-insertar para mantener consistencia entre instancias.
-- La FK en profile_permissions tiene ON DELETE CASCADE, así que
-- borrar permissions limpia profile_permissions automáticamente.
DELETE FROM `permissions`
WHERE `code` IN (
  'PROJECTS_ACCESS', 'PROJECTS_ASSIGN', 'PROJECTS_CREATE',
  'ETC_ACCESS', 'ETC_EDIT',
  'ESTIMATED_PROJECTS_ACCESS',
  'DASHBOARD_EVM_ACCESS', 'DASHBOARD_HOURS_ACCESS',
  'REPORTS_ACCESS',
  'ADMIN_ACCESS',
  'SETTINGS_ACCESS'
);

INSERT INTO `permissions` (`module_id`, `name`, `code`, `description`, `active`, `created_at`, `updated_at`)
VALUES
  ((SELECT id FROM `modules` WHERE code = 'PROJECTS'),           'Visualizar Proyectos',     'PROJECTS_ACCESS',           'Acceder a la visualización de proyectos',           1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'PROJECTS'),           'Asignación de Proyectos',  'PROJECTS_ASSIGN',           'Acceder a la asignación de proyectos a usuarios',   1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'PROJECTS'),           'Alta de Proyectos',        'PROJECTS_CREATE',           'Crear y administrar altas de proyectos',            1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ETC'),                'Carga ETC',                'ETC_ACCESS',                'Acceder a la carga de ETC',                         1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ETC'),                'Edición ETC',              'ETC_EDIT',                  'Crear, modificar y eliminar registros ETC',         1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ESTIMATED_PROJECTS'), 'Proyectos Estimados',      'ESTIMATED_PROJECTS_ACCESS', 'Acceder al flujo de proyectos estimados',           1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'DASHBOARD'),          'Dashboard EVM',            'DASHBOARD_EVM_ACCESS',      'Acceder al dashboard EVM',                          1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'DASHBOARD'),          'Dashboard Horas',          'DASHBOARD_HOURS_ACCESS',    'Acceder al dashboard de horas',                     1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'REPORTS'),            'Reportes',                 'REPORTS_ACCESS',            'Acceder al módulo de reportes',                     1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ADMINISTRATION'),     'Administración',           'ADMIN_ACCESS',              'Acceder a funcionalidades administrativas',         1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'SETTINGS'),           'Configuración',            'SETTINGS_ACCESS',           'Acceder a configuración del sistema',               1, NOW(), NOW());

-- ── Profile-Permissions (matriz RF-03) ───────────────────────
-- profile_permissions ya quedó limpia por CASCADE al borrar permissions.
INSERT INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT p.id, perm.id, a.id, NOW(), NOW()
FROM `profiles` p
CROSS JOIN `permissions` perm
CROSS JOIN `actions` a
WHERE a.code = 'create'
AND (
  -- Administrador: acceso total
  (p.code = 'admin' AND perm.code IN (
    'PROJECTS_ACCESS', 'PROJECTS_ASSIGN', 'PROJECTS_CREATE',
    'ETC_ACCESS', 'ETC_EDIT',
    'ESTIMATED_PROJECTS_ACCESS',
    'DASHBOARD_EVM_ACCESS', 'DASHBOARD_HOURS_ACCESS',
    'REPORTS_ACCESS', 'ADMIN_ACCESS', 'SETTINGS_ACCESS'
  ))
  -- Soporte: Configuración
  OR (p.code = 'soporte' AND perm.code IN (
    'SETTINGS_ACCESS'
  ))
  -- Operaciones Gerente: Operación + Análisis + Reportería
  OR (p.code = 'ops_gerente' AND perm.code IN (
    'PROJECTS_ACCESS', 'PROJECTS_ASSIGN',
    'ETC_ACCESS', 'ETC_EDIT',
    'ESTIMATED_PROJECTS_ACCESS',
    'DASHBOARD_EVM_ACCESS', 'DASHBOARD_HOURS_ACCESS',
    'REPORTS_ACCESS'
  ))
  -- Operaciones Líderes: Operación + Análisis + Reportería
  OR (p.code = 'ops_lider' AND perm.code IN (
    'PROJECTS_ACCESS', 'PROJECTS_ASSIGN',
    'ETC_ACCESS', 'ETC_EDIT',
    'ESTIMATED_PROJECTS_ACCESS',
    'DASHBOARD_EVM_ACCESS', 'DASHBOARD_HOURS_ACCESS',
    'REPORTS_ACCESS'
  ))
  -- Administración: Administración + Reportería
  OR (p.code = 'administracion' AND perm.code IN (
    'ADMIN_ACCESS', 'PROJECTS_CREATE',
    'REPORTS_ACCESS'
  ))
);
