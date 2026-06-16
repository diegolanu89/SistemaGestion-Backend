-- =====================================================================
-- 03_schema.sql
-- change_audit_log — registro de auditoría transversal (RF-11).
--
-- Aprobado por el cliente luego de revisión interna con Sergio.
-- La aprobación fue condicional a no generar impacto en performance:
--   * `user_id` NO tiene FK → el log de auditoría sobrevive eliminaciones
--     de usuarios y evita contención de locks con la tabla users.
--   * `module` se guarda como string (sin FK) para desacoplar la
--     evolución del esquema RBAC del historial de auditoría.
--   * Los índices apuntan a los tres patrones de acceso esperados:
--       (a) "historial de este registro"  → (entity, record_id, ts)
--       (b) "qué hizo este usuario"       → (user_id, ts)
--       (c) barridos de retención         → (ts)
--   * La población ocurre vía interceptor en el backend
--     (EF Core SaveChangesInterceptor + hook explícito en AuthController
--     para login/logout). Sin llamadas de auditoría desde controllers.
--
-- Nota de nomenclatura: la columna es `event_type` (no `action`) para
-- evitar confusión con la tabla RBAC `actions` — esta columna representa
-- el tipo de evento auditado (create/update/delete/login/logout), que es
-- un concepto distinto al nivel de acceso.
-- =====================================================================

USE pm_timesheet_evm;

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
