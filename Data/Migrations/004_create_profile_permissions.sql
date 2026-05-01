-- =====================================================================
-- 004_create_profile_permissions.sql
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
