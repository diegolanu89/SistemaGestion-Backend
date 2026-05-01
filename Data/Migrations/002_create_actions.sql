-- =====================================================================
-- 002_create_actions.sql
-- actions — extensible lookup of access levels.
--
-- Hierarchical model: each level includes everything the lower level
-- allows.
--   level 1  read_only  → view
--   level 2  edit       → view + edit
--   level 3  create     → view + edit + create + delete (full)
--
-- The action belongs to the (profile, permission) assignment, not to
-- the permission itself (see profile_permissions.action_id in 004).
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
