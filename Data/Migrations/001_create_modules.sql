-- =====================================================================
-- 001_create_modules.sql
-- modules — system module taxonomy (RF-03 / RF-04).
-- Approved by client 2026-04-29 (normalized RBAC).
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
