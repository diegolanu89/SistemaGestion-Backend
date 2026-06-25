-- ============================================================
-- 002_rename_clockify_to_timesheet.sql
-- Renombra tablas y columnas clockify_* -> timesheet_*.
-- Idempotente: solo ejecuta si clockify_users todavía existe.
-- Aplica a clientes con la base anterior a Febrero 2026
-- (antes de la migración Laravel #27).
-- Clientes que ya corrieron el rename no necesitan esto.
-- ============================================================

USE pm_timesheet_evm;

DROP PROCEDURE IF EXISTS _mig_002;
DELIMITER $$
CREATE PROCEDURE _mig_002()
BEGIN
  -- Solo ejecuta si las tablas clockify_* aún existen
  IF EXISTS (
    SELECT 1 FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'clockify_users'
  ) THEN
    SET FOREIGN_KEY_CHECKS = 0;

    RENAME TABLE
      clockify_clients         TO timesheet_clients,
      clockify_project_filters TO timesheet_project_filters,
      clockify_projects        TO timesheet_projects,
      clockify_time_entries    TO timesheet_time_entries,
      clockify_users           TO timesheet_users;

    IF EXISTS (
      SELECT 1 FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME   = 'timesheet_projects'
        AND COLUMN_NAME  = 'clockify_project_id'
    ) THEN
      ALTER TABLE timesheet_projects
        RENAME COLUMN clockify_project_id TO timesheet_project_id;
    END IF;

    IF EXISTS (
      SELECT 1 FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME   = 'timesheet_time_entries'
        AND COLUMN_NAME  = 'clockify_time_entry_id'
    ) THEN
      ALTER TABLE timesheet_time_entries
        RENAME COLUMN clockify_time_entry_id TO timesheet_time_entry_id;
    END IF;

    IF EXISTS (
      SELECT 1 FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME   = 'timesheet_users'
        AND COLUMN_NAME  = 'clockify_user_id'
    ) THEN
      ALTER TABLE timesheet_users
        RENAME COLUMN clockify_user_id TO timesheet_user_id;
    END IF;

    IF EXISTS (
      SELECT 1 FROM information_schema.COLUMNS
      WHERE TABLE_SCHEMA = DATABASE()
        AND TABLE_NAME   = 'project_intake_records'
        AND COLUMN_NAME  = 'leader_clockify_user_id'
    ) THEN
      ALTER TABLE project_intake_records
        RENAME COLUMN leader_clockify_user_id    TO leader_timesheet_user_id;
      ALTER TABLE project_intake_records
        RENAME COLUMN requires_clockify_creation TO requires_timesheet_creation;
      ALTER TABLE project_intake_records
        RENAME COLUMN clockify_record_id         TO timesheet_record_id;
    END IF;

    SET FOREIGN_KEY_CHECKS = 1;
  END IF;
END$$
DELIMITER ;

CALL _mig_002();
DROP PROCEDURE IF EXISTS _mig_002;
