-- ============================================================
-- 006_add_project_tracking_tables.sql
-- Crea project_trackings y project_tracking_updates.
-- Idempotente: CREATE TABLE IF NOT EXISTS.
-- ============================================================

USE pm_timesheet_evm;

CREATE TABLE IF NOT EXISTS `project_trackings` (
  `id`                 BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_id`         BIGINT UNSIGNED NOT NULL,
  `start_date`         DATE            DEFAULT NULL,
  `planned_end_date`   DATE            DEFAULT NULL,
  `actual_end_date`    DATE            DEFAULT NULL,
  `implementation_date` DATE           DEFAULT NULL,
  `created_at`         TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`         TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_trackings_project_id_unique` (`project_id`),
  CONSTRAINT `project_trackings_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `project_tracking_updates` (
  `id`                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_tracking_id` BIGINT UNSIGNED NOT NULL,
  `change_end_date`     DATE            DEFAULT NULL,
  `observations`        TEXT            DEFAULT NULL,
  `created_at`          TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`          TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `project_tracking_updates_project_tracking_id_foreign` (`project_tracking_id`),
  CONSTRAINT `project_tracking_updates_project_tracking_id_foreign`
    FOREIGN KEY (`project_tracking_id`) REFERENCES `project_trackings` (`id`)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
