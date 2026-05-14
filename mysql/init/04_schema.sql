
CREATE TABLE IF NOT EXISTS pm_clockify_evm.modules (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(191) NOT NULL,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS pm_clockify_evm.actions (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    code VARCHAR(100) NOT NULL UNIQUE,
    level INT NOT NULL DEFAULT 1,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS pm_clockify_evm.permissions (
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

CREATE TABLE IF NOT EXISTS pm_clockify_evm.profile_permissions (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    profile_id BIGINT UNSIGNED NOT NULL,
    permission_id BIGINT UNSIGNED NOT NULL,
    action_id BIGINT UNSIGNED NOT NULL,
    created_at TIMESTAMP NULL,
    updated_at TIMESTAMP NULL,
    PRIMARY KEY (id)
);

INSERT IGNORE INTO pm_clockify_evm.profiles (name, code, description, created_at, updated_at)
VALUES ('Administrador', 'ADMIN', 'Perfil administrador del sistema', NOW(), NOW());

INSERT IGNORE INTO pm_clockify_evm.modules (code, name, active, created_at, updated_at)
VALUES
('PROJECTS', 'Gestión de Proyectos', 1, NOW(), NOW()),
('ETC', 'Carga ETC', 1, NOW(), NOW()),
('ESTIMATED_PROJECTS', 'Proyectos Estimados', 1, NOW(), NOW()),
('DASHBOARD', 'Dashboards', 1, NOW(), NOW()),
('REPORTS', 'Reportería', 1, NOW(), NOW()),
('ADMINISTRATION', 'Administración', 1, NOW(), NOW()),
('SETTINGS', 'Configuración', 1, NOW(), NOW());

-- Las acciones (read_only, edit, create) ya las siembra 02_schema.sql con level 1/2/3.
-- No re-insertamos acá para no chocar con el UNIQUE en `level`.

DELETE pp
FROM pm_clockify_evm.profile_permissions pp
INNER JOIN pm_clockify_evm.profiles p ON p.id = pp.profile_id
WHERE p.code IN ('admin', 'lider', 'soporte', 'ops_gerente', 'ops_lider', 'administracion');

DELETE perm
FROM pm_clockify_evm.permissions perm
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

INSERT INTO pm_clockify_evm.permissions (module_id, name, code, description, active, created_at, updated_at)
VALUES
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'PROJECTS' LIMIT 1), 'Visualizar Proyectos', 'PROJECTS_ACCESS', 'Permite acceder a la visualización de proyectos', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'PROJECTS' LIMIT 1), 'Asignación de Proyectos', 'PROJECTS_ASSIGN', 'Permite acceder a la asignación de proyectos', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'PROJECTS' LIMIT 1), 'Alta de Proyectos', 'PROJECTS_CREATE', 'Permite crear y administrar altas de proyectos', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'ETC' LIMIT 1), 'Carga ETC', 'ETC_ACCESS', 'Permite acceder a la carga de ETC', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'ETC' LIMIT 1), 'Edición ETC', 'ETC_EDIT', 'Permite crear, modificar y eliminar registros ETC', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'ESTIMATED_PROJECTS' LIMIT 1), 'Alta de Proyectos Estimados', 'ESTIMATED_PROJECTS_ACCESS', 'Permite acceder al flujo de proyectos estimados', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'DASHBOARD' LIMIT 1), 'Dashboard EVM', 'DASHBOARD_EVM_ACCESS', 'Permite acceder al dashboard EVM', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'DASHBOARD' LIMIT 1), 'Dashboard Horas', 'DASHBOARD_HOURS_ACCESS', 'Permite acceder al dashboard de horas', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'REPORTS' LIMIT 1), 'Reportes', 'REPORTS_ACCESS', 'Permite acceder al módulo de reportes', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'ADMINISTRATION' LIMIT 1), 'Administración', 'ADMIN_ACCESS', 'Permite acceder a funcionalidades administrativas', 1, NOW(), NOW()),
((SELECT id FROM pm_clockify_evm.modules WHERE code = 'SETTINGS' LIMIT 1), 'Configuración', 'SETTINGS_ACCESS', 'Permite acceder a configuración del sistema', 1, NOW(), NOW());

-- Matriz perfil → permisos (action_id=3 = 'create', acceso total al permiso)
-- Mapeo módulo → permisos (según ERS y sidebar):
--   Operación      = PROJECTS_ACCESS, PROJECTS_ASSIGN, ETC_ACCESS, ETC_EDIT, ESTIMATED_PROJECTS_ACCESS
--   Análisis       = DASHBOARD_EVM_ACCESS, DASHBOARD_HOURS_ACCESS
--   Reportería     = REPORTS_ACCESS
--   Administración = PROJECTS_CREATE (Alta de proyectos), ADMIN_ACCESS
--   Configuración  = SETTINGS_ACCESS
INSERT INTO pm_clockify_evm.profile_permissions (profile_id, permission_id, action_id, created_at, updated_at)
SELECT p.id, perm.id, a.id, NOW(), NOW()
FROM pm_clockify_evm.profiles p
CROSS JOIN pm_clockify_evm.permissions perm
CROSS JOIN pm_clockify_evm.actions a
WHERE a.code = 'create'
AND (
    -- 1. Administrador: acceso total
    (p.code = 'admin' AND perm.code IN (
        'PROJECTS_ACCESS','PROJECTS_ASSIGN','PROJECTS_CREATE',
        'ETC_ACCESS','ETC_EDIT',
        'ESTIMATED_PROJECTS_ACCESS','DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS',
        'REPORTS_ACCESS','ADMIN_ACCESS','SETTINGS_ACCESS'
    ))
    -- 2. Usuario Lider: solo dashboards
    OR (p.code = 'lider' AND perm.code IN (
        'DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS'
    ))
    -- 3. Soporte: solo reportería
    OR (p.code = 'soporte' AND perm.code IN (
        'REPORTS_ACCESS'
    ))
    -- 4. Operaciones Gerente: Operación + Análisis + Reportería
    OR (p.code = 'ops_gerente' AND perm.code IN (
        'PROJECTS_ACCESS','PROJECTS_ASSIGN',
        'ETC_ACCESS','ETC_EDIT',
        'ESTIMATED_PROJECTS_ACCESS',
        'DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS',
        'REPORTS_ACCESS'
    ))
    -- 5. Operaciones Líder: Operación + Análisis
    OR (p.code = 'ops_lider' AND perm.code IN (
        'PROJECTS_ACCESS','PROJECTS_ASSIGN',
        'ETC_ACCESS','ETC_EDIT',
        'ESTIMATED_PROJECTS_ACCESS',
        'DASHBOARD_EVM_ACCESS','DASHBOARD_HOURS_ACCESS'
    ))
    -- 6. Administración: Administración + Configuración
    --    (Alta de proyectos pertenece a módulo Administración según ERS — pide PROJECTS_CREATE)
    OR (p.code = 'administracion' AND perm.code IN (
        'ADMIN_ACCESS','PROJECTS_CREATE','SETTINGS_ACCESS'
    ))
);

UPDATE pm_clockify_evm.users u
INNER JOIN pm_clockify_evm.profiles p ON p.code = 'ADMIN'
SET u.profile_id = p.id
WHERE u.email = 'diego@test.com';