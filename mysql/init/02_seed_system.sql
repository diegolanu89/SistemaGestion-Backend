-- ============================================================
-- 02_seed_system.sql — Datos de sistema requeridos para funcionar.
-- Sin datos mock. Necesario tanto en dev como en prod.
-- Idempotente: INSERT IGNORE en todos los casos.
-- ============================================================

USE pm_timesheet_evm;
SET NAMES utf8mb4;

-- ── Perfiles de usuario ───────────────────────────────────────

INSERT IGNORE INTO `profiles` (`id`, `name`, `code`, `description`, `created_at`, `updated_at`) VALUES
  (1, 'Administrador',       'admin',         'Acceso total al sistema',                                     NULL, NULL),
  (2, 'Soporte',             'soporte',       'Acceso total sobre módulo Configuración',                     NULL, NULL),
  (3, 'Operaciones Gerente', 'ops_gerente',   'Acceso total sobre Operación, Análisis y Reportería',         NULL, NULL),
  (4, 'Operaciones Líderes', 'ops_lider',     'Acceso total sobre Operación, Análisis y Reportería',         NULL, NULL),
  (5, 'Administración',      'administracion','Acceso total sobre Administración y Reportería',              NULL, NULL);

-- ── Calendario laboral 2026 (Argentina) ──────────────────────

INSERT IGNORE INTO `working_days_calendar`
  (`id`, `month_key`, `month_label`, `year`, `month`, `total_days`, `working_days`,
   `hours_month`, `holiday_days`, `holidays_list`, `notes`, `created_at`, `updated_at`)
VALUES
  (1,  '2026-01', 'Enero 2026',      2026,  1, 31, 21, 168.00, 1, '["2026-01-01"]',                       NULL,           NULL, NULL),
  (2,  '2026-02', 'Febrero 2026',    2026,  2, 28, 18, 144.00, 2, '["2026-02-16","2026-02-17"]',           'Carnaval',     NULL, NULL),
  (3,  '2026-03', 'Marzo 2026',      2026,  3, 31, 21, 168.00, 1, '["2026-03-24"]',                       NULL,           NULL, NULL),
  (4,  '2026-04', 'Abril 2026',      2026,  4, 30, 20, 160.00, 2, '["2026-04-02","2026-04-03"]',           'Semana Santa', NULL, NULL),
  (5,  '2026-05', 'Mayo 2026',       2026,  5, 31, 19, 152.00, 2, '["2026-05-01","2026-05-25"]',           NULL,           NULL, NULL),
  (6,  '2026-06', 'Junio 2026',      2026,  6, 30, 21, 168.00, 1, '["2026-06-17"]',                       'Jun 20 cae sábado', NULL, NULL),
  (7,  '2026-07', 'Julio 2026',      2026,  7, 31, 22, 176.00, 1, '["2026-07-09"]',                       NULL,           NULL, NULL),
  (8,  '2026-08', 'Agosto 2026',     2026,  8, 31, 20, 160.00, 1, '["2026-08-17"]',                       NULL,           NULL, NULL),
  (9,  '2026-09', 'Septiembre 2026', 2026,  9, 30, 22, 176.00, 0, NULL,                                   NULL,           NULL, NULL),
  (10, '2026-10', 'Octubre 2026',    2026, 10, 31, 21, 168.00, 1, '["2026-10-12"]',                       NULL,           NULL, NULL),
  (11, '2026-11', 'Noviembre 2026',  2026, 11, 30, 20, 160.00, 1, '["2026-11-20"]',                       NULL,           NULL, NULL),
  (12, '2026-12', 'Diciembre 2026',  2026, 12, 31, 21, 168.00, 2, '["2026-12-08","2026-12-25"]',           NULL,           NULL, NULL);

-- ── RBAC — módulos ────────────────────────────────────────────

INSERT IGNORE INTO `modules` (`code`, `name`, `description`, `active`, `created_at`, `updated_at`) VALUES
  ('PROJECTS',           'Gestión de Proyectos',  'Pantallas del módulo Operación — proyectos',          1, NOW(), NOW()),
  ('ETC',                'Carga ETC',             'Pantallas del módulo Operación — carga ETC',          1, NOW(), NOW()),
  ('ESTIMATED_PROJECTS', 'Proyectos Estimados',   'Pantallas del módulo Operación — proyectos estimados',1, NOW(), NOW()),
  ('DASHBOARD',          'Dashboards',            'Pantallas del módulo Análisis',                       1, NOW(), NOW()),
  ('REPORTS',            'Reportería',            'Pantallas del módulo Reportería',                     1, NOW(), NOW()),
  ('ADMINISTRATION',     'Administración',        'Pantallas del módulo Administración',                 1, NOW(), NOW()),
  ('SETTINGS',           'Configuración',         'Pantallas del módulo Configuración',                  1, NOW(), NOW());

-- ── RBAC — acciones ───────────────────────────────────────────

INSERT IGNORE INTO `actions` (`code`, `name`, `description`, `level`, `active`, `created_at`, `updated_at`) VALUES
  ('read_only', 'Sólo lectura', 'Permite ver la pantalla. No puede editar, crear ni eliminar.', 1, 1, NOW(), NOW()),
  ('edit',      'Edición',      'Permite ver y editar registros existentes.',                   2, 1, NOW(), NOW()),
  ('create',    'Creación',     'Acceso total: ver, editar, crear y eliminar.',                 3, 1, NOW(), NOW());

-- ── RBAC — permisos ───────────────────────────────────────────

INSERT IGNORE INTO `permissions` (`module_id`, `name`, `code`, `description`, `active`, `created_at`, `updated_at`) VALUES
  ((SELECT id FROM `modules` WHERE code = 'PROJECTS'),           'Visualizar Proyectos',    'PROJECTS_ACCESS',           'Acceder a la visualización de proyectos',           1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'PROJECTS'),           'Asignación de Proyectos', 'PROJECTS_ASSIGN',           'Acceder a la asignación de proyectos a usuarios',   1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'PROJECTS'),           'Alta de Proyectos',       'PROJECTS_CREATE',           'Crear y administrar altas de proyectos',            1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ETC'),                'Carga ETC',               'ETC_ACCESS',                'Acceder a la carga de ETC',                         1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ETC'),                'Edición ETC',             'ETC_EDIT',                  'Crear, modificar y eliminar registros ETC',         1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ESTIMATED_PROJECTS'), 'Proyectos Estimados',     'ESTIMATED_PROJECTS_ACCESS', 'Acceder al flujo de proyectos estimados',           1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'DASHBOARD'),          'Dashboard EVM',           'DASHBOARD_EVM_ACCESS',      'Acceder al dashboard EVM',                          1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'DASHBOARD'),          'Dashboard Horas',         'DASHBOARD_HOURS_ACCESS',    'Acceder al dashboard de horas',                     1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'REPORTS'),            'Reportes',                'REPORTS_ACCESS',            'Acceder al módulo de reportes',                     1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'ADMINISTRATION'),     'Administración',          'ADMIN_ACCESS',              'Acceder a funcionalidades administrativas',         1, NOW(), NOW()),
  ((SELECT id FROM `modules` WHERE code = 'SETTINGS'),           'Configuración',           'SETTINGS_ACCESS',           'Acceder a configuración del sistema',               1, NOW(), NOW());

-- ── RBAC — matriz de permisos por perfil ─────────────────────
--
-- Administrador        → todos los módulos
-- Soporte              → Configuración
-- Operaciones Gerente  → Operación + Análisis + Reportería
-- Operaciones Líderes  → Operación + Análisis + Reportería
-- Administración       → Administración + Reportería

INSERT IGNORE INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT p.id, perm.id, a.id, NOW(), NOW()
FROM `profiles` p
CROSS JOIN `permissions` perm
CROSS JOIN `actions` a
WHERE a.code = 'create'
  AND (
    (p.code = 'admin'         AND perm.code IN ('PROJECTS_ACCESS','PROJECTS_ASSIGN','PROJECTS_CREATE','ETC_ACCESS','ETC_EDIT','ESTIMATED_PROJECTS_ACCESS','DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS','REPORTS_ACCESS','ADMIN_ACCESS','SETTINGS_ACCESS'))
 OR (p.code = 'soporte'       AND perm.code IN ('SETTINGS_ACCESS'))
 OR (p.code = 'ops_gerente'   AND perm.code IN ('PROJECTS_ACCESS','PROJECTS_ASSIGN','ETC_ACCESS','ETC_EDIT','ESTIMATED_PROJECTS_ACCESS','DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS','REPORTS_ACCESS'))
 OR (p.code = 'ops_lider'     AND perm.code IN ('PROJECTS_ACCESS','PROJECTS_ASSIGN','ETC_ACCESS','ETC_EDIT','ESTIMATED_PROJECTS_ACCESS','DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS','REPORTS_ACCESS'))
 OR (p.code = 'administracion'AND perm.code IN ('ADMIN_ACCESS','PROJECTS_CREATE','REPORTS_ACCESS'))
  );

-- ── Referencias de Intake ─────────────────────────────────────

INSERT IGNORE INTO `project_intake_type_refs`
  (`code`, `label`, `description`, `internal_label`, `secondary_label`, `registration_label`,
   `requires_business_status_date`, `requires_actual_end_date`, `requires_commercial_fields`, `is_active`, `created_at`, `updated_at`)
VALUES
  ('30', 'Desarrollo', 'Proyectos de desarrollo de software',
   'Nro. Proyecto Desarrollo', 'Nro. Proyecto Comercial', 'Fecha de Alta',
   1, 0, 0, 1, NOW(), NOW());

INSERT IGNORE INTO `project_intake_category_refs` (`code`, `label`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
  ('PRE',   'Pre-venta',          'Proyectos en etapa de preventa',            1, NOW(), NOW()),
  ('DES',   'Desarrollo',         'Proyectos en desarrollo activo',            1, NOW(), NOW()),
  ('SOP',   'Soporte',            'Proyectos en etapa de soporte',             1, NOW(), NOW()),
  ('I+D',   'I+D',                'Investigación y desarrollo',                1, NOW(), NOW()),
  ('SWF',   'SWFactory',          'Proyectos bajo modalidad SW Factory',       1, NOW(), NOW()),
  ('DEV',   'DevOps',             'Proyectos de infraestructura y DevOps',     1, NOW(), NOW()),
  ('STAFF', 'Staff Augmentation', 'Proyectos de staffing',                     1, NOW(), NOW()),
  ('PROXY', 'Proxy Int.',         'Proyectos de proxy internacional',          1, NOW(), NOW());

INSERT IGNORE INTO `project_intake_status_refs` (`code`, `label`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
  ('INGRESO',    'Ingreso',    'Proyecto ingresado y en evaluación',        1, NOW(), NOW()),
  ('EN_CURSO',   'En curso',   'Proyecto aprobado y en ejecución',          1, NOW(), NOW()),
  ('PERDIDO',    'Perdido',    'Proyecto no adjudicado o cancelado',        1, NOW(), NOW()),
  ('CERRADO',    'Cerrado',    'Proyecto finalizado y cerrado formalmente', 1, NOW(), NOW()),
  ('SUSPENDIDO', 'Suspendido', 'Proyecto pausado temporalmente',            1, NOW(), NOW());
