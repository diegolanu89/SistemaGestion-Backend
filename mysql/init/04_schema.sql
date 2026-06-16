
USE pm_timesheet_evm;

-- =====================================================================
-- Módulos del sistema (RF-03).
-- Mapeo ERS → códigos internos:
--   Operación      → PROJECTS, ETC, ESTIMATED_PROJECTS
--   Análisis       → DASHBOARD
--   Reportería     → REPORTS
--   Administración → ADMINISTRATION
--   Configuración  → SETTINGS
-- =====================================================================

CREATE TABLE IF NOT EXISTS pm_timesheet_evm.modules (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(191) NOT NULL,
    description TEXT NULL,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS pm_timesheet_evm.actions (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT NULL,
    level INT NOT NULL DEFAULT 1,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS pm_timesheet_evm.permissions (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    module_id BIGINT UNSIGNED NOT NULL,
    name VARCHAR(191) NOT NULL DEFAULT '',
    code VARCHAR(150) NOT NULL UNIQUE,
    description TEXT NULL,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS pm_timesheet_evm.profile_permissions (
    profile_id BIGINT UNSIGNED NOT NULL,
    permission_id BIGINT UNSIGNED NOT NULL,
    action_id BIGINT UNSIGNED NOT NULL,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (profile_id, permission_id)
);

-- =====================================================================
-- Seed: módulos
-- =====================================================================

INSERT IGNORE INTO pm_timesheet_evm.modules (code, name, description, active, created_at, updated_at)
VALUES
('PROJECTS',          'Gestión de Proyectos',   'Pantallas del módulo Operación — proyectos',         1, NOW(), NOW()),
('ETC',               'Carga ETC',              'Pantallas del módulo Operación — carga ETC',         1, NOW(), NOW()),
('ESTIMATED_PROJECTS','Proyectos Estimados',    'Pantallas del módulo Operación — proyectos estimados',1, NOW(), NOW()),
('DASHBOARD',         'Dashboards',             'Pantallas del módulo Análisis',                      1, NOW(), NOW()),
('REPORTS',           'Reportería',             'Pantallas del módulo Reportería',                    1, NOW(), NOW()),
('ADMINISTRATION',    'Administración',         'Pantallas del módulo Administración',                1, NOW(), NOW()),
('SETTINGS',          'Configuración',          'Pantallas del módulo Configuración',                 1, NOW(), NOW());

-- =====================================================================
-- Seed: acciones (niveles jerárquicos)
--   level 1 = solo lectura
--   level 2 = edición
--   level 3 = acceso total (crear / editar / eliminar)
-- =====================================================================

INSERT IGNORE INTO pm_timesheet_evm.actions (code, name, description, level, active, created_at, updated_at)
VALUES
('read_only', 'Sólo lectura', 'Permite ver la pantalla. No puede editar, crear ni eliminar.', 1, 1, NOW(), NOW()),
('edit',      'Edición',      'Permite ver y editar registros existentes.',                   2, 1, NOW(), NOW()),
('create',    'Creación',     'Acceso total: ver, editar, crear y eliminar.',                 3, 1, NOW(), NOW());

-- =====================================================================
-- Seed: permisos (uno por pantalla/recurso protegido)
-- =====================================================================

-- Limpiar permisos anteriores para re-seedear limpio
DELETE perm
FROM pm_timesheet_evm.permissions perm
WHERE perm.code IN (
    'PROJECTS_ACCESS',
    'PROJECTS_ASSIGN',
    'PROJECTS_CREATE',
    'ETC_ACCESS',
    'ETC_EDIT',
    'ESTIMATED_PROJECTS_ACCESS',
    'DASHBOARD_EVM_ACCESS',
    'DASHBOARD_HOURS_ACCESS',
    'REPORTS_ACCESS',
    'ADMIN_ACCESS',
    'SETTINGS_ACCESS'
);

INSERT INTO pm_timesheet_evm.permissions (module_id, name, code, description, active, created_at, updated_at)
VALUES
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'PROJECTS'),           'Visualizar Proyectos',          'PROJECTS_ACCESS',          'Acceder a la visualización de proyectos',          1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'PROJECTS'),           'Asignación de Proyectos',       'PROJECTS_ASSIGN',          'Acceder a la asignación de proyectos a usuarios',  1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'PROJECTS'),           'Alta de Proyectos',             'PROJECTS_CREATE',          'Crear y administrar altas de proyectos',           1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'ETC'),                'Carga ETC',                     'ETC_ACCESS',               'Acceder a la carga de ETC',                        1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'ETC'),                'Edición ETC',                   'ETC_EDIT',                 'Crear, modificar y eliminar registros ETC',        1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'ESTIMATED_PROJECTS'), 'Proyectos Estimados',           'ESTIMATED_PROJECTS_ACCESS','Acceder al flujo de proyectos estimados',          1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'DASHBOARD'),          'Dashboard EVM',                 'DASHBOARD_EVM_ACCESS',     'Acceder al dashboard EVM',                         1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'DASHBOARD'),          'Dashboard Horas',               'DASHBOARD_HOURS_ACCESS',   'Acceder al dashboard de horas',                    1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'REPORTS'),            'Reportes',                      'REPORTS_ACCESS',           'Acceder al módulo de reportes',                    1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'ADMINISTRATION'),     'Administración',                'ADMIN_ACCESS',             'Acceder a funcionalidades administrativas',        1, NOW(), NOW()),
((SELECT id FROM pm_timesheet_evm.modules WHERE code = 'SETTINGS'),           'Configuración',                 'SETTINGS_ACCESS',          'Acceder a configuración del sistema',              1, NOW(), NOW());

-- =====================================================================
-- Seed: profile_permissions — matriz RF-03
--
-- Módulos ERS → permisos internos:
--   Operación      = PROJECTS_ACCESS, PROJECTS_ASSIGN, ETC_ACCESS, ETC_EDIT, ESTIMATED_PROJECTS_ACCESS
--   Análisis       = DASHBOARD_EVM_ACCESS, DASHBOARD_HOURS_ACCESS
--   Reportería     = REPORTS_ACCESS
--   Administración = ADMIN_ACCESS, PROJECTS_CREATE
--   Configuración  = SETTINGS_ACCESS
--
-- Matriz de acceso (todos con nivel 'create' = acceso total):
--   Administrador        → todos los módulos
--   Soporte              → Configuración
--   Operaciones Gerente  → Operación + Análisis + Reportería
--   Operaciones Líderes  → Operación + Análisis + Reportería
--   Administración       → Administración + Reportería
-- =====================================================================

-- Limpiar asignaciones anteriores para re-seedear limpio
DELETE pp
FROM pm_timesheet_evm.profile_permissions pp
INNER JOIN pm_timesheet_evm.profiles p ON p.id = pp.profile_id
WHERE p.code IN ('admin', 'soporte', 'ops_gerente', 'ops_lider', 'administracion');

INSERT INTO pm_timesheet_evm.profile_permissions (profile_id, permission_id, action_id, created_at, updated_at)
SELECT p.id, perm.id, a.id, NOW(), NOW()
FROM pm_timesheet_evm.profiles p
CROSS JOIN pm_timesheet_evm.permissions perm
CROSS JOIN pm_timesheet_evm.actions a
WHERE a.code = 'create'
AND (
    -- Administrador: acceso total a todos los módulos
    (p.code = 'admin' AND perm.code IN (
        'PROJECTS_ACCESS', 'PROJECTS_ASSIGN', 'PROJECTS_CREATE',
        'ETC_ACCESS', 'ETC_EDIT',
        'ESTIMATED_PROJECTS_ACCESS',
        'DASHBOARD_EVM_ACCESS', 'DASHBOARD_HOURS_ACCESS',
        'REPORTS_ACCESS',
        'ADMIN_ACCESS',
        'SETTINGS_ACCESS'
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
