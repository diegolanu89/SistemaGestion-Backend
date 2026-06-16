-- =====================================================================
-- 02_schema.sql
-- RBAC normalizado (RF-02 / RF-03) — DDL únicamente.
-- Los datos seed (módulos, acciones, permisos, profile_permissions)
-- se gestionan en 04_schema.sql que es la fuente de verdad del RBAC.
-- =====================================================================

USE pm_timesheet_evm;

-- =====================================================================
-- modules — taxonomía de módulos del sistema (RF-03).
-- =====================================================================

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

-- =====================================================================
-- actions — niveles de acceso jerárquicos.
--   level 1  read_only  → solo ver
--   level 2  edit       → ver + editar
--   level 3  create     → acceso total (ver + editar + crear + eliminar)
-- =====================================================================

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

-- =====================================================================
-- permissions — catálogo de pantallas/recursos protegidos.
-- =====================================================================

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

-- =====================================================================
-- profile_permissions — N:N perfil ↔ permiso con nivel de acción.
-- PK compuesta (profile_id, permission_id): un nivel por par.
-- =====================================================================

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
