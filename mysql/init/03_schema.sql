-- =====================================================================
-- 03_schema.sql
-- change_audit_log — cross-cutting audit log (RF-11).
--
-- Approved by client after internal review with Sergio. Approval was
-- conditional on no performance impact, hence:
--   * `user_id` has NO FK → audit log survives user deletes and avoids
--     lock contention with the users table.
--   * `module` is stored as string (no FK) to decouple RBAC schema
--     evolution from audit history.
--   * Indexes target the three expected access patterns:
--       (a) "history of this record"      → (entity, record_id, ts)
--       (b) "what did this user do"       → (user_id, ts)
--       (c) retention sweeps              → (ts)
--   * Population happens via interceptor in the backend (EF Core
--     SaveChangesInterceptor + explicit hook in AuthController for
--     login/logout). NO audit calls from controllers.
--
-- Naming note: the column is `event_type` (not `action`) to avoid
-- confusion with the RBAC `actions` table — this column carries the
-- type of audited event (create/update/delete/login/logout), which is
-- a different concept from access level.
-- =====================================================================

USE pm_clockify_evm;

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `change_audit_log` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `ts` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `user_id` bigint(20) unsigned DEFAULT NULL,
  `user_email` varchar(191) DEFAULT NULL,
  `module` varchar(50) DEFAULT NULL,
  `entity` varchar(100) NOT NULL,
  `record_id` varchar(64) DEFAULT NULL,
  `event_type` ENUM('create','update','delete','login','logout') NOT NULL,
  `old_value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL
    CHECK (`old_value` IS NULL OR JSON_VALID(`old_value`)),
  `new_value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL
    CHECK (`new_value` IS NULL OR JSON_VALID(`new_value`)),
  `request_id` varchar(64) DEFAULT NULL,
  `ip` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_audit_entity_record` (`entity`, `record_id`, `ts`),
  KEY `idx_audit_user_ts` (`user_id`, `ts`),
  KEY `idx_audit_ts` (`ts`),
  KEY `idx_audit_module_ts` (`module`, `ts`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

COMMIT;
