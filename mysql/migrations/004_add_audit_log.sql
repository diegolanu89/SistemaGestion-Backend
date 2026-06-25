-- ============================================================
-- 004_add_audit_log.sql
-- Crea la tabla de auditoría transversal (RF-11).
-- Idempotente: CREATE TABLE IF NOT EXISTS.
-- `user_id` intencional sin FK para sobrevivir bajas de usuarios.
-- ============================================================

USE pm_timesheet_evm;

CREATE TABLE IF NOT EXISTS `change_audit_log` (
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
