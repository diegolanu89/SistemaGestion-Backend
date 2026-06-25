-- ============================================================
-- 005_add_intake_tables.sql
-- Crea las tablas de Intake de proyectos (RF-04 / RF-07).
-- Solo DDL — sin filas. Ver 005b_seed_intake_refs.sql para seed.
-- Idempotente: CREATE TABLE IF NOT EXISTS.
-- Orden: refs primero (sin FK entre sí), luego project_intake_records.
-- ============================================================

USE pm_timesheet_evm;

CREATE TABLE IF NOT EXISTS `project_intake_category_refs` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code`        VARCHAR(40)     NOT NULL,
  `label`       VARCHAR(120)    NOT NULL,
  `description` VARCHAR(255)    DEFAULT NULL,
  `is_active`   TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_category_refs_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `project_intake_type_refs` (
  `id`                         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code`                       VARCHAR(2)   NOT NULL,
  `label`                      VARCHAR(120) NOT NULL,
  `description`                VARCHAR(255) DEFAULT NULL,
  `internal_label`             VARCHAR(255) NOT NULL,
  `secondary_label`            VARCHAR(255) NOT NULL,
  `registration_label`         VARCHAR(255) NOT NULL,
  `requires_business_status_date` TINYINT(1) NOT NULL DEFAULT 1,
  `requires_actual_end_date`   TINYINT(1)   NOT NULL DEFAULT 0,
  `requires_commercial_fields` TINYINT(1)   NOT NULL DEFAULT 0,
  `is_active`                  TINYINT(1)   NOT NULL DEFAULT 1,
  `created_at`                 TIMESTAMP    NULL DEFAULT NULL,
  `updated_at`                 TIMESTAMP    NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_type_refs_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `project_intake_status_refs` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code`        VARCHAR(40)     NOT NULL,
  `label`       VARCHAR(120)    NOT NULL,
  `description` VARCHAR(255)    DEFAULT NULL,
  `is_active`   TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_status_refs_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `project_intake_records` (
  `id`                          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_type`                VARCHAR(2)      DEFAULT NULL,
  `internal_project_number`     VARCHAR(120)    DEFAULT NULL,
  `secondary_project_number`    VARCHAR(120)    DEFAULT NULL,
  `registration_date`           DATE            DEFAULT NULL,
  `client_name`                 VARCHAR(255)    DEFAULT NULL,
  `client_id`                   BIGINT UNSIGNED DEFAULT NULL,
  `project_name`                VARCHAR(255)    DEFAULT NULL,
  `category_code`               VARCHAR(20)     DEFAULT NULL,
  `project_status_code`         VARCHAR(30)     DEFAULT NULL,
  `business_status_date`        DATE            DEFAULT NULL,
  `estimated_end_date`          DATE            DEFAULT NULL,
  `actual_end_date`             DATE            DEFAULT NULL,
  `commercial_status`           VARCHAR(120)    DEFAULT NULL,
  `leader_timesheet_user_id`    BIGINT UNSIGNED DEFAULT NULL,
  `observations`                TEXT            DEFAULT NULL,
  `requires_timesheet_creation` TINYINT(1)      NOT NULL DEFAULT 0,
  `timesheet_record_id`         BIGINT UNSIGNED DEFAULT NULL,
  `created_by`                  BIGINT UNSIGNED DEFAULT NULL,
  `updated_by`                  BIGINT UNSIGNED DEFAULT NULL,
  `is_active`                   TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`                  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`                  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `project_intake_records_project_type_created_at_index`      (`project_type`, `created_at`),
  KEY `project_intake_records_project_status_code_index`          (`project_status_code`),
  KEY `project_intake_records_category_code_index`                (`category_code`),
  KEY `project_intake_records_requires_clockify_creation_index`   (`requires_timesheet_creation`),
  KEY `idx_intake_is_active`                                      (`is_active`),
  KEY `fk_intake_clockify_project`                                (`timesheet_record_id`),
  KEY `idx_intake_client_id`                                      (`client_id`),
  KEY `idx_intake_leader`                                         (`leader_timesheet_user_id`),
  CONSTRAINT `fk_intake_category_code`
    FOREIGN KEY (`category_code`) REFERENCES `project_intake_category_refs` (`code`)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_intake_client`
    FOREIGN KEY (`client_id`) REFERENCES `timesheet_clients` (`id`)
    ON DELETE SET NULL,
  CONSTRAINT `fk_intake_clockify_project`
    FOREIGN KEY (`timesheet_record_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE SET NULL,
  CONSTRAINT `fk_intake_leader`
    FOREIGN KEY (`leader_timesheet_user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE SET NULL,
  CONSTRAINT `fk_intake_project_type`
    FOREIGN KEY (`project_type`) REFERENCES `project_intake_type_refs` (`code`)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_intake_status_code`
    FOREIGN KEY (`project_status_code`) REFERENCES `project_intake_status_refs` (`code`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

