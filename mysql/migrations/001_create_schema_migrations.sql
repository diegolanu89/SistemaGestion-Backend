-- ============================================================
-- 001_create_schema_migrations.sql
-- Tabla de control para seguimiento de migraciones aplicadas.
-- Idempotente: CREATE TABLE IF NOT EXISTS.
-- ============================================================

USE pm_timesheet_evm;

CREATE TABLE IF NOT EXISTS `schema_migrations` (
  `id`             INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `migration_name` VARCHAR(255) NOT NULL,
  `applied_at`     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_schema_migrations_name` (`migration_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
