-- ============================================================
-- Migración COMPLETA: clockify -> timesheet (tablas, columnas y nombre de base)
-- ============================================================
-- Para bases con datos EXISTENTES (staging/prod). En dev local NO hace falta:
-- el init de mysql/init/*.sql ya crea la base pm_timesheet_evm con tablas
-- timesheet_* directamente, así que `down -v` + up alcanza.
--
-- La migración tiene DOS pasos. El PASO 1 (este archivo SQL) renombra tablas y
-- columnas in-place dentro de la base actual. El PASO 2 (shell, abajo) renombra
-- la base — MySQL no tiene RENAME DATABASE, se hace con dump/restore.
--
-- Alcance: tablas + columnas + nombre de base. NO renombra nombres internos de
-- constraints/índices (siguen funcionando); así DB fresca (init) y DB migrada
-- quedan funcionalmente idénticas.
-- ============================================================

-- ── PASO 1 — tablas y columnas (correr sobre la base origen, ej. pm_clockify_evm) ──
-- MySQL 8: RENAME TABLE actualiza las FK de tablas hijas; RENAME COLUMN actualiza
-- índices y FKs que usan la columna.

SET FOREIGN_KEY_CHECKS = 0;

RENAME TABLE
  clockify_clients         TO timesheet_clients,
  clockify_project_filters TO timesheet_project_filters,
  clockify_projects        TO timesheet_projects,
  clockify_time_entries    TO timesheet_time_entries,
  clockify_users           TO timesheet_users;

ALTER TABLE timesheet_projects     RENAME COLUMN clockify_project_id        TO timesheet_project_id;
ALTER TABLE timesheet_time_entries RENAME COLUMN clockify_time_entry_id     TO timesheet_time_entry_id;
ALTER TABLE timesheet_users        RENAME COLUMN clockify_user_id           TO timesheet_user_id;
ALTER TABLE project_intake_records RENAME COLUMN leader_clockify_user_id    TO leader_timesheet_user_id;
ALTER TABLE project_intake_records RENAME COLUMN requires_clockify_creation TO requires_timesheet_creation;
ALTER TABLE project_intake_records RENAME COLUMN clockify_record_id         TO timesheet_record_id;

SET FOREIGN_KEY_CHECKS = 1;

-- ── PASO 2 — renombrar la base pm_clockify_evm -> pm_timesheet_evm ──
-- MySQL no soporta RENAME DATABASE. Método portable (dump/restore), corriendo
-- DESPUÉS del PASO 1, desde el host o el contenedor db:
--
--   mysql -u bdt_user -pbdt_user -e \
--     "CREATE DATABASE pm_timesheet_evm CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
--
--   mysqldump -u bdt_user -pbdt_user --routines --triggers --single-transaction \
--     pm_clockify_evm | mysql -u bdt_user -pbdt_user pm_timesheet_evm
--
--   # Verificar datos en pm_timesheet_evm y recién entonces:
--   mysql -u bdt_user -pbdt_user -e "DROP DATABASE pm_clockify_evm;"
--
-- Luego apuntar el backend a la base nueva (ConnectionStrings__Default / DB_DATABASE).
