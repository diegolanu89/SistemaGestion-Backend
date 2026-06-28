-- ============================================================
-- 01_schema.sql — DDL completo del sistema BDT EVM.
-- Sin datos. MySQL ejecuta este archivo automáticamente al
-- crear el volumen por primera vez (docker-entrypoint-initdb.d).
-- ============================================================

CREATE DATABASE IF NOT EXISTS pm_timesheet_evm
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE pm_timesheet_evm;

-- ── Auth ──────────────────────────────────────────────────────

CREATE TABLE `profiles` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name`        VARCHAR(191)    NOT NULL,
  `code`        VARCHAR(50)     NOT NULL,
  `description` TEXT            DEFAULT NULL,
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `profiles_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `users` (
  `id`                BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name`              VARCHAR(191)    NOT NULL,
  `email`             VARCHAR(191)    NOT NULL,
  `email_verified_at` TIMESTAMP       NULL DEFAULT NULL,
  `password`          VARCHAR(191)    NOT NULL,
  `profile_id`        BIGINT UNSIGNED DEFAULT NULL,
  `active`            TINYINT(1)      NOT NULL DEFAULT 1,
  `remember_token`    VARCHAR(100)    DEFAULT NULL,
  `created_at`        TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`        TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `users_email_unique` (`email`),
  KEY `users_profile_id_foreign` (`profile_id`),
  CONSTRAINT `users_profile_id_foreign`
    FOREIGN KEY (`profile_id`) REFERENCES `profiles` (`id`)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `password_reset_tokens` (
  `email`      VARCHAR(191) NOT NULL,
  `token`      VARCHAR(191) NOT NULL,
  `created_at` TIMESTAMP    NULL DEFAULT NULL,
  PRIMARY KEY (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `personal_access_tokens` (
  `id`             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `tokenable_type` VARCHAR(191)    NOT NULL,
  `tokenable_id`   BIGINT UNSIGNED NOT NULL,
  `name`           VARCHAR(191)    NOT NULL,
  `token`          VARCHAR(64)     NOT NULL,
  `abilities`      TEXT            DEFAULT NULL,
  `last_used_at`   TIMESTAMP       NULL DEFAULT NULL,
  `expires_at`     TIMESTAMP       NULL DEFAULT NULL,
  `created_at`     TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`     TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `personal_access_tokens_token_unique` (`token`),
  KEY `personal_access_tokens_tokenable_type_tokenable_id_index` (`tokenable_type`, `tokenable_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `failed_jobs` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `uuid`       VARCHAR(191)    NOT NULL,
  `connection` TEXT            NOT NULL,
  `queue`      TEXT            NOT NULL,
  `payload`    LONGTEXT        NOT NULL,
  `exception`  LONGTEXT        NOT NULL,
  `failed_at`  TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `failed_jobs_uuid_unique` (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── RBAC ──────────────────────────────────────────────────────

CREATE TABLE `modules` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code`        VARCHAR(100)    NOT NULL,
  `name`        VARCHAR(191)    NOT NULL,
  `description` TEXT            DEFAULT NULL,
  `active`      TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `modules_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `actions` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code`        VARCHAR(100)    NOT NULL,
  `name`        VARCHAR(100)    NOT NULL,
  `description` TEXT            DEFAULT NULL,
  `level`       INT             NOT NULL DEFAULT 1,
  `active`      TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `actions_code_unique` (`code`),
  UNIQUE KEY `actions_level_unique` (`level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `permissions` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `module_id`   BIGINT UNSIGNED NOT NULL,
  `name`        VARCHAR(191)    NOT NULL DEFAULT '',
  `code`        VARCHAR(150)    NOT NULL,
  `description` TEXT            DEFAULT NULL,
  `active`      TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `permissions_code_unique` (`code`),
  KEY `permissions_module_id_foreign` (`module_id`),
  CONSTRAINT `permissions_module_id_foreign`
    FOREIGN KEY (`module_id`) REFERENCES `modules` (`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `profile_permissions` (
  `profile_id`    BIGINT UNSIGNED NOT NULL,
  `permission_id` BIGINT UNSIGNED NOT NULL,
  `action_id`     BIGINT UNSIGNED NOT NULL,
  `created_at`    TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`    TIMESTAMP       NULL DEFAULT NULL,
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

-- ── Timesheet ─────────────────────────────────────────────────

CREATE TABLE `timesheet_clients` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name`        VARCHAR(191)    NOT NULL,
  `external_id` VARCHAR(64)     DEFAULT NULL,
  `status`      ENUM('activo','inactivo') NOT NULL DEFAULT 'activo',
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_clients_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `timesheet_users` (
  `id`                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `timesheet_user_id`   VARCHAR(64)     DEFAULT NULL,
  `name`                VARCHAR(255)    NOT NULL,
  `email`               VARCHAR(191)    DEFAULT NULL,
  `role`                VARCHAR(50)     DEFAULT NULL,
  `active`              TINYINT(1)      NOT NULL DEFAULT 1,
  `default_month_hours` DECIMAL(8,2)    DEFAULT NULL,
  `created_at`          TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`          TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_clockify` (`timesheet_user_id`),
  KEY `idx_users_email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `timesheet_projects` (
  `id`                   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `timesheet_project_id` VARCHAR(128)    NOT NULL,
  `name`                 VARCHAR(255)    NOT NULL,
  `code`                 VARCHAR(100)    DEFAULT NULL,
  `client_id`            BIGINT UNSIGNED DEFAULT NULL,
  `status`               ENUM('activo','pausado','cerrado') NOT NULL DEFAULT 'activo',
  `start_date`           DATE            DEFAULT NULL,
  `end_date_planned`     DATE            DEFAULT NULL,
  `end_date_actual`      DATE            DEFAULT NULL,
  `bac_base_hours`       DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `bac_base_cost`        DECIMAL(12,2)   NOT NULL DEFAULT '0.00',
  `bac_total_hours`      DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `bac_total_cost`       DECIMAL(12,2)   NOT NULL DEFAULT '0.00',
  `hourly_rate`          DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `etc_calculation_mode` ENUM('manual','automatic') NOT NULL DEFAULT 'manual',
  `created_at`           TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`           TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_projects_clockify` (`timesheet_project_id`),
  KEY `idx_projects_client` (`client_id`),
  KEY `idx_projects_status` (`status`),
  CONSTRAINT `clockify_projects_client_id_foreign`
    FOREIGN KEY (`client_id`) REFERENCES `timesheet_clients` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `timesheet_project_filters` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_id` BIGINT UNSIGNED NOT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_project_filters_project` (`project_id`),
  CONSTRAINT `clockify_project_filters_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── EVM ───────────────────────────────────────────────────────

CREATE TABLE `change_requests` (
  `id`                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_id`          BIGINT UNSIGNED NOT NULL,
  `code`                VARCHAR(50)     NOT NULL,
  `title`               VARCHAR(255)    NOT NULL,
  `description`         TEXT            DEFAULT NULL,
  `requested_by`        VARCHAR(255)    DEFAULT NULL,
  `requested_date`      DATE            NOT NULL,
  `status`              ENUM('propuesto','aprobado','rechazado','implementado') NOT NULL DEFAULT 'propuesto',
  `bac_hours_increment` DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `bac_cost_increment`  DECIMAL(12,2)   NOT NULL DEFAULT '0.00',
  `approved_by`         VARCHAR(255)    DEFAULT NULL,
  `approved_date`       DATE            DEFAULT NULL,
  `created_at`          TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`          TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_cr_project_code` (`project_id`, `code`),
  KEY `idx_cr_status` (`status`),
  KEY `idx_cr_requested_date` (`requested_date`),
  CONSTRAINT `change_requests_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `etc_snapshots` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_id` BIGINT UNSIGNED NOT NULL,
  `version`    INT UNSIGNED    NOT NULL DEFAULT 1,
  `label`      VARCHAR(100)    DEFAULT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `etc_snapshots_project_id_version_index` (`project_id`, `version`),
  CONSTRAINT `etc_snapshots_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `timesheet_time_entries` (
  `id`                      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `timesheet_time_entry_id` VARCHAR(64)     NOT NULL,
  `project_id`              BIGINT UNSIGNED NOT NULL,
  `user_id`                 BIGINT UNSIGNED DEFAULT NULL,
  `description`             VARCHAR(255)    DEFAULT NULL,
  `start_time`              DATETIME        NOT NULL,
  `end_time`                DATETIME        NOT NULL,
  `duration_hours`          DECIMAL(10,3)   NOT NULL,
  `billable`                TINYINT(1)      NOT NULL DEFAULT 1,
  `change_request_id`       BIGINT UNSIGNED DEFAULT NULL,
  `source_raw`              JSON            DEFAULT NULL,
  `created_at`              TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`              TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_te_clockify` (`timesheet_time_entry_id`),
  KEY `idx_te_project` (`project_id`),
  KEY `idx_te_user` (`user_id`),
  KEY `idx_te_start_time` (`start_time`),
  KEY `idx_te_cr` (`change_request_id`),
  CONSTRAINT `clockify_time_entries_change_request_id_foreign`
    FOREIGN KEY (`change_request_id`) REFERENCES `change_requests` (`id`)
    ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `clockify_time_entries_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `clockify_time_entries_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `etc_records` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_id`  BIGINT UNSIGNED NOT NULL,
  `snapshot_id` BIGINT UNSIGNED DEFAULT NULL,
  `user_id`     BIGINT UNSIGNED DEFAULT NULL,
  `user_name`   VARCHAR(255)    DEFAULT NULL,
  `month_key`   VARCHAR(7)      NOT NULL,
  `month_label` VARCHAR(30)     NOT NULL,
  `hours`       DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_etc_project` (`project_id`),
  KEY `idx_etc_user` (`user_id`),
  KEY `idx_etc_month` (`month_key`),
  KEY `idx_etc_project_month` (`project_id`, `month_key`),
  KEY `idx_etc_project_user_month` (`project_id`, `user_id`, `month_key`),
  KEY `etc_records_snapshot_id_index` (`snapshot_id`),
  CONSTRAINT `etc_records_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `etc_records_snapshot_id_foreign`
    FOREIGN KEY (`snapshot_id`) REFERENCES `etc_snapshots` (`id`)
    ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `etc_records_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── Control de cambios ────────────────────────────────────────

CREATE TABLE `project_trackings` (
  `id`                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `project_id`          BIGINT UNSIGNED NOT NULL,
  `start_date`          DATE            DEFAULT NULL,
  `planned_end_date`    DATE            DEFAULT NULL,
  `actual_end_date`     DATE            DEFAULT NULL,
  `implementation_date` DATE            DEFAULT NULL,
  `created_at`          TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`          TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_trackings_project_id_unique` (`project_id`),
  CONSTRAINT `project_trackings_project_id_foreign`
    FOREIGN KEY (`project_id`) REFERENCES `timesheet_projects` (`id`)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `project_tracking_updates` (
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

-- ── Capacidad y RRHH ──────────────────────────────────────────

CREATE TABLE `working_days_calendar` (
  `id`            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `month_key`     VARCHAR(7)      NOT NULL,
  `month_label`   VARCHAR(30)     NOT NULL,
  `year`          INT             NOT NULL,
  `month`         INT             NOT NULL,
  `total_days`    INT             NOT NULL,
  `working_days`  INT             NOT NULL,
  `hours_month`   DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `holiday_days`  INT             NOT NULL DEFAULT 0,
  `holidays_list` TEXT            DEFAULT NULL,
  `notes`         TEXT            DEFAULT NULL,
  `created_at`    TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`    TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_wdc_year_month` (`year`, `month`),
  UNIQUE KEY `working_days_calendar_month_key_unique` (`month_key`),
  KEY `idx_wdc_month_key` (`month_key`),
  KEY `idx_wdc_year_month` (`year`, `month`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_leaders` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`    BIGINT UNSIGNED NOT NULL,
  `leader_id`  BIGINT UNSIGNED NOT NULL,
  `start_date` DATE            NOT NULL,
  `end_date`   DATE            DEFAULT NULL,
  `notes`      TEXT            DEFAULT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ul_user_leader_start` (`user_id`, `leader_id`, `start_date`),
  KEY `idx_ul_user` (`user_id`),
  KEY `idx_ul_leader` (`leader_id`),
  KEY `idx_ul_user_start` (`user_id`, `start_date`),
  KEY `idx_ul_leader_start` (`leader_id`, `start_date`),
  CONSTRAINT `user_leaders_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `user_leaders_leader_id_foreign`
    FOREIGN KEY (`leader_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_monthly_capacities` (
  `id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`     BIGINT UNSIGNED NOT NULL,
  `month_key`   VARCHAR(7)      NOT NULL,
  `month_label` VARCHAR(30)     DEFAULT NULL,
  `hours`       DECIMAL(8,2)    NOT NULL DEFAULT '0.00',
  `created_at`  TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`  TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_umc_user_month` (`user_id`, `month_key`),
  KEY `idx_umc_month_key` (`month_key`),
  CONSTRAINT `user_monthly_capacities_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_monthly_status` (
  `id`                BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`           BIGINT UNSIGNED NOT NULL,
  `month_key`         VARCHAR(7)      NOT NULL,
  `year`              INT             NOT NULL,
  `month`             INT             NOT NULL,
  `status`            ENUM('activo','inactivo','vacaciones','licencia','baja') NOT NULL DEFAULT 'activo',
  `status_start_date` DATE            DEFAULT NULL,
  `status_end_date`   DATE            DEFAULT NULL,
  `days_in_status`    INT             NOT NULL DEFAULT 0,
  `notes`             TEXT            DEFAULT NULL,
  `created_at`        TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`        TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ums_user_month` (`user_id`, `month_key`),
  KEY `idx_ums_user_month` (`user_id`, `month_key`),
  KEY `idx_ums_user_year_month` (`user_id`, `year`, `month`),
  KEY `idx_ums_month_key` (`month_key`),
  KEY `idx_ums_status` (`status`),
  CONSTRAINT `user_monthly_status_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_vacation_periods` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`    BIGINT UNSIGNED NOT NULL,
  `date_from`  DATE            NOT NULL,
  `date_to`    DATE            NOT NULL,
  `total_days` INT UNSIGNED    NOT NULL DEFAULT 0,
  `notes`      TEXT            DEFAULT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `user_vacation_periods_user_id_date_from_index` (`user_id`, `date_from`),
  CONSTRAINT `user_vacation_periods_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_hours_summary` (
  `id`             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`        BIGINT UNSIGNED NOT NULL,
  `project_id`     INT UNSIGNED    DEFAULT NULL,
  `client_id`      INT UNSIGNED    DEFAULT NULL,
  `leader_id`      BIGINT UNSIGNED DEFAULT NULL,
  `month_key`      VARCHAR(7)      NOT NULL,
  `year`           INT             NOT NULL,
  `month`          INT             NOT NULL,
  `duration_hours` DECIMAL(10,3)   NOT NULL DEFAULT '0.000',
  `expected_hours` DECIMAL(10,3)   NOT NULL DEFAULT '0.000',
  `notes`          TEXT            DEFAULT NULL,
  `created_at`     TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`     TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_uhs_user_project_month` (`user_id`, `project_id`, `month_key`),
  KEY `idx_uhs_user_month` (`user_id`, `month_key`),
  KEY `idx_uhs_project_month` (`project_id`, `month_key`),
  KEY `idx_uhs_client_month` (`client_id`, `month_key`),
  KEY `idx_uhs_leader_month` (`leader_id`, `month_key`),
  KEY `idx_uhs_month_key` (`month_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── Dashboard ─────────────────────────────────────────────────

CREATE TABLE `app_user_visible_projects` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`    BIGINT UNSIGNED NOT NULL,
  `project_id` BIGINT UNSIGNED NOT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `app_user_visible_projects_user_id_project_id_unique` (`user_id`, `project_id`),
  CONSTRAINT `app_user_visible_projects_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_dashboard_filters` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`    BIGINT UNSIGNED NOT NULL,
  `name`       VARCHAR(191)    NOT NULL,
  `leader_id`  VARCHAR(50)     DEFAULT NULL,
  `month_keys` JSON            DEFAULT NULL,
  `project_id` VARCHAR(50)     DEFAULT NULL,
  `source_type` VARCHAR(20)     DEFAULT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `user_dashboard_filters_user_id_foreign` (`user_id`),
  CONSTRAINT `user_dashboard_filters_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── Pipeline ──────────────────────────────────────────────────

CREATE TABLE `potencial_clients` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name`       VARCHAR(255)    NOT NULL,
  `created_at` TIMESTAMP       NULL DEFAULT NULL,
  `updated_at` TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `potencial_projects` (
  `id`                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name`                VARCHAR(255)    NOT NULL,
  `code`                VARCHAR(100)    DEFAULT NULL,
  `potencial_client_id` BIGINT UNSIGNED NOT NULL,
  `created_at`          TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`          TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `potencial_projects_potencial_client_id_foreign` (`potencial_client_id`),
  CONSTRAINT `potencial_projects_potencial_client_id_foreign`
    FOREIGN KEY (`potencial_client_id`) REFERENCES `potencial_clients` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `potencial_project_allocations` (
  `id`                   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `potencial_project_id` BIGINT UNSIGNED NOT NULL,
  `month_key`            VARCHAR(7)      NOT NULL,
  `month_label`          VARCHAR(30)     DEFAULT NULL,
  `user_id`              BIGINT UNSIGNED DEFAULT NULL,
  `user_name`            VARCHAR(255)    DEFAULT NULL,
  `hours`                DECIMAL(10,2)   NOT NULL DEFAULT '0.00',
  `created_at`           TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`           TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `potencial_project_allocations_user_id_foreign` (`user_id`),
  KEY `potencial_alloc_project_month` (`potencial_project_id`, `month_key`),
  KEY `potencial_alloc_month` (`month_key`),
  CONSTRAINT `potencial_project_allocations_potencial_project_id_foreign`
    FOREIGN KEY (`potencial_project_id`) REFERENCES `potencial_projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `potencial_project_allocations_user_id_foreign`
    FOREIGN KEY (`user_id`) REFERENCES `timesheet_users` (`id`)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── Intake de proyectos ───────────────────────────────────────

CREATE TABLE `project_intake_category_refs` (
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

CREATE TABLE `project_intake_type_refs` (
  `id`                            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code`                          VARCHAR(2)      NOT NULL,
  `label`                         VARCHAR(120)    NOT NULL,
  `description`                   VARCHAR(255)    DEFAULT NULL,
  `internal_label`                VARCHAR(255)    NOT NULL,
  `secondary_label`               VARCHAR(255)    NOT NULL,
  `registration_label`            VARCHAR(255)    NOT NULL,
  `requires_business_status_date` TINYINT(1)      NOT NULL DEFAULT 1,
  `requires_actual_end_date`      TINYINT(1)      NOT NULL DEFAULT 0,
  `requires_commercial_fields`    TINYINT(1)      NOT NULL DEFAULT 0,
  `is_active`                     TINYINT(1)      NOT NULL DEFAULT 1,
  `created_at`                    TIMESTAMP       NULL DEFAULT NULL,
  `updated_at`                    TIMESTAMP       NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `project_intake_type_refs_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `project_intake_status_refs` (
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

CREATE TABLE `project_intake_records` (
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
  KEY `idx_intake_project_type_created`   (`project_type`, `created_at`),
  KEY `idx_intake_project_status_code`    (`project_status_code`),
  KEY `idx_intake_category_code`          (`category_code`),
  KEY `idx_intake_requires_timesheet`     (`requires_timesheet_creation`),
  KEY `idx_intake_is_active`              (`is_active`),
  KEY `idx_intake_timesheet_record`       (`timesheet_record_id`),
  KEY `idx_intake_client_id`              (`client_id`),
  KEY `idx_intake_leader`                 (`leader_timesheet_user_id`),
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

-- ── Auditoría ─────────────────────────────────────────────────

CREATE TABLE `change_audit_log` (
  `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `ts`         TIMESTAMP(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `user_id`    BIGINT UNSIGNED DEFAULT NULL,
  `user_email` VARCHAR(191)    DEFAULT NULL,
  `module`     VARCHAR(50)     DEFAULT NULL,
  `entity`     VARCHAR(100)    NOT NULL,
  `record_id`  VARCHAR(64)     DEFAULT NULL,
  `event_type` ENUM('create','update','delete','login','logout') NOT NULL,
  `old_value`  LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL
    CHECK (`old_value` IS NULL OR JSON_VALID(`old_value`)),
  `new_value`  LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL
    CHECK (`new_value` IS NULL OR JSON_VALID(`new_value`)),
  `request_id` VARCHAR(64)     DEFAULT NULL,
  `ip`         VARCHAR(45)     DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_audit_entity_record` (`entity`, `record_id`, `ts`),
  KEY `idx_audit_user_ts`       (`user_id`, `ts`),
  KEY `idx_audit_ts`            (`ts`),
  KEY `idx_audit_module_ts`     (`module`, `ts`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
