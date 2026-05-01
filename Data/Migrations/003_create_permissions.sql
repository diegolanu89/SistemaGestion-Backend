-- =====================================================================
-- 003_create_permissions.sql
-- permissions — catalog of protected screens/resources.
--
-- IMPORTANT: the access action (read_only / edit / create) is NOT in
-- this table. It lives in profile_permissions.action_id (script 004),
-- because the action is a property of the assignment, not of the
-- resource itself.
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
