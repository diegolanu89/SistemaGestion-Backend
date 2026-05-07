-- =====================================================================
-- 02_schema.sql
-- RBAC normalizado (RF-02 / RF-03) — perfiles ↔ permisos.
-- Consolida los scripts 001-004 originales de Data/Migrations.
-- Approved by client 2026-04-29 (normalized RBAC).
-- =====================================================================

USE pm_clockify_evm;

-- =====================================================================
-- modules — system module taxonomy (RF-03 / RF-04).
-- =====================================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `modules` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `code` varchar(50) NOT NULL,
  `description` text DEFAULT NULL,
  `active` tinyint(1) NOT NULL DEFAULT 1,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `modules_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed: 5 ERS modules. Display names in Spanish (UI language); codes in English.
INSERT INTO `modules` (`name`, `code`, `description`, `active`, `created_at`, `updated_at`) VALUES
  ('Operación',      'operations',     'Gestión operativa de proyectos y horas',  1, NOW(), NOW()),
  ('Análisis',       'analysis',       'Tableros analíticos, EVM, ETC',           1, NOW(), NOW()),
  ('Reportería',     'reports',        'Reportes y exportaciones',                1, NOW(), NOW()),
  ('Administración', 'administration', 'ABM de usuarios, roles, calendarios',     1, NOW(), NOW()),
  ('Configuración',  'configuration',  'Parámetros del sistema e integraciones',  1, NOW(), NOW())
ON DUPLICATE KEY UPDATE `name` = VALUES(`name`), `description` = VALUES(`description`), `updated_at` = NOW();

COMMIT;

-- =====================================================================
-- actions — extensible lookup of access levels.
--
-- Hierarchical model: each level includes everything the lower level
-- allows.
--   level 1  read_only  → view
--   level 2  edit       → view + edit
--   level 3  create     → view + edit + create + delete (full)
--
-- The action belongs to the (profile, permission) assignment, not to
-- the permission itself (see profile_permissions.action_id below).
-- Authorization check is "current_level >= required_level", not an
-- enumeration of atomic actions.
-- =====================================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `actions` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(50) NOT NULL,
  `name` varchar(100) NOT NULL,
  `description` text DEFAULT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT 1,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `actions_code_unique` (`code`),
  UNIQUE KEY `actions_level_unique` (`level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed: 3 initial levels. If the client later asks for a new action
-- that fits the hierarchy (e.g. "approve" between edit and create), it
-- can be inserted here without touching code. If a new action does NOT
-- fit the hierarchy, evaluate moving to a capability-flag model.
INSERT INTO `actions` (`code`, `name`, `description`, `level`, `active`, `created_at`, `updated_at`) VALUES
  ('read_only', 'Sólo lectura', 'Permite ver la pantalla. No puede editar, crear ni eliminar.',          1, 1, NOW(), NOW()),
  ('edit',      'Edición',      'Permite ver y editar registros existentes. No puede crear ni eliminar.', 2, 1, NOW(), NOW()),
  ('create',    'Creación',     'Acceso total: ver, editar, crear y eliminar.',                          3, 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE `name` = VALUES(`name`), `description` = VALUES(`description`), `level` = VALUES(`level`), `updated_at` = NOW();

COMMIT;

-- =====================================================================
-- permissions — catalog of protected screens/resources.
--
-- IMPORTANT: the access action (read_only / edit / create) is NOT in
-- this table. It lives in profile_permissions.action_id, because the
-- action is a property of the assignment, not of the resource itself.
--
-- Code convention:
--   * Module-wide permission:    `<module_code>`              e.g. `operations`
--   * Screen-level permission:   `<module_code>.<screen>`     e.g. `operations.projects`
-- Screen-level permissions are added incrementally as the frontend
-- declares each view (RF-04). This script seeds the 5 module-wide
-- baselines only.
-- =====================================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `permissions` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `module_id` bigint(20) unsigned NOT NULL,
  `name` varchar(150) NOT NULL,
  `code` varchar(100) NOT NULL,
  `description` text DEFAULT NULL,
  `active` tinyint(1) NOT NULL DEFAULT 1,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `permissions_code_unique` (`code`),
  KEY `permissions_module_id_foreign` (`module_id`),
  CONSTRAINT `permissions_module_id_foreign`
    FOREIGN KEY (`module_id`) REFERENCES `modules` (`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed: 1 module-wide baseline per module (5 rows).
INSERT INTO `permissions` (`module_id`, `name`, `code`, `description`, `active`, `created_at`, `updated_at`)
SELECT
  m.id,
  CONCAT(m.name, ' (módulo)') AS name,
  m.code                       AS code,
  CONCAT('Acceso al módulo ', m.name) AS description,
  1,
  NOW(),
  NOW()
FROM `modules` m
ON DUPLICATE KEY UPDATE `description` = VALUES(`description`), `updated_at` = NOW();

COMMIT;

-- =====================================================================
-- profile_permissions — N:N profile (role) ↔ permission, with action level.
--
-- Naming note: the role concept is implemented by the existing
-- `profiles` table. The FK column is `profile_id` to match
-- `users.profile_id` already in the schema.
--
-- Composite PK (profile_id, permission_id) — a single action level per
-- (profile, permission) pair. If the system later needs more than one
-- level on the same screen for the same role, change the PK; for now
-- the rule is "one level per pair".
--
-- Implementation default: every role starts at action = 'create'
-- (level 3, full access) on its assigned modules until the client or
-- the team define narrower restrictions. This preserves the exact
-- behavior of Services/ProfileCatalog.cs at cutover.
-- =====================================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `profile_permissions` (
  `profile_id` bigint(20) unsigned NOT NULL,
  `permission_id` bigint(20) unsigned NOT NULL,
  `action_id` bigint(20) unsigned NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`profile_id`, `permission_id`),
  KEY `profile_permissions_permission_id_foreign` (`permission_id`),
  KEY `profile_permissions_action_id_foreign` (`action_id`),
  CONSTRAINT `profile_permissions_profile_id_foreign`
    FOREIGN KEY (`profile_id`) REFERENCES `profiles` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `profile_permissions_permission_id_foreign`
    FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `profile_permissions_action_id_foreign`
    FOREIGN KEY (`action_id`) REFERENCES `actions` (`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------
-- Pre-seed of profiles: the dump shows `profiles AUTO_INCREMENT=4`
-- (only 3 rows). We make sure the 5 codes ProfileCatalog.cs recognizes
-- exist before seeding the junction.
-- ---------------------------------------------------------------------
INSERT IGNORE INTO `profiles` (`name`, `code`, `description`, `created_at`, `updated_at`) VALUES
  ('Administrador',         'admin',          'Acceso total al sistema',           NOW(), NOW()),
  ('Soporte',               'soporte',        'Acceso a módulo Reportería',        NOW(), NOW()),
  ('Operaciones — Gerente', 'ops_gerente',    'Operación + Análisis + Reportería', NOW(), NOW()),
  ('Operaciones — Líder',   'ops_lider',      'Operación + Análisis',              NOW(), NOW()),
  ('Administración',        'administracion', 'Administración + Configuración',    NOW(), NOW());

-- ---------------------------------------------------------------------
-- Junction seed (all rows at action 'create' = full access).
-- Replicates the map in Services/ProfileCatalog.cs:
--   admin           → all modules
--   administracion  → administration + configuration
--   ops_gerente     → operations + analysis + reports
--   ops_lider       → operations + analysis
--   soporte         → reports
-- Total expected: 5 + 2 + 3 + 2 + 1 = 13 rows.
-- ---------------------------------------------------------------------

-- admin: all permissions
INSERT IGNORE INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT pf.id, pe.id, ac.id, NOW(), NOW()
FROM `profiles` pf
CROSS JOIN `permissions` pe
JOIN `actions` ac ON ac.code = 'create'
WHERE pf.code = 'admin';

-- administracion: administration + configuration modules
INSERT IGNORE INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT pf.id, pe.id, ac.id, NOW(), NOW()
FROM `profiles` pf
JOIN `modules` m ON m.code IN ('administration', 'configuration')
JOIN `permissions` pe ON pe.module_id = m.id
JOIN `actions` ac ON ac.code = 'create'
WHERE pf.code = 'administracion';

-- ops_gerente: operations + analysis + reports modules
INSERT IGNORE INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT pf.id, pe.id, ac.id, NOW(), NOW()
FROM `profiles` pf
JOIN `modules` m ON m.code IN ('operations', 'analysis', 'reports')
JOIN `permissions` pe ON pe.module_id = m.id
JOIN `actions` ac ON ac.code = 'create'
WHERE pf.code = 'ops_gerente';

-- ops_lider: operations + analysis modules
INSERT IGNORE INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT pf.id, pe.id, ac.id, NOW(), NOW()
FROM `profiles` pf
JOIN `modules` m ON m.code IN ('operations', 'analysis')
JOIN `permissions` pe ON pe.module_id = m.id
JOIN `actions` ac ON ac.code = 'create'
WHERE pf.code = 'ops_lider';

-- soporte: reports module
INSERT IGNORE INTO `profile_permissions` (`profile_id`, `permission_id`, `action_id`, `created_at`, `updated_at`)
SELECT pf.id, pe.id, ac.id, NOW(), NOW()
FROM `profiles` pf
JOIN `modules` m ON m.code = 'reports'
JOIN `permissions` pe ON pe.module_id = m.id
JOIN `actions` ac ON ac.code = 'create'
WHERE pf.code = 'soporte';

COMMIT;
